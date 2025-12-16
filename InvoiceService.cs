using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using OpenMicroFiscal.Extensions;
using OpenMicroFiscal.Models;

namespace OpenMicroFiscal;

public sealed class InvoiceService(
    HttpClient httpClient,
    FiscalizationSettings settings,
    ProvideCertificate provideCertificate)
{
    public async Task<CreateInvoiceResult> CreateInvoiceAsync(
        CreateInvoiceRequest createInvoiceRequest,
        CancellationToken cancellationToken = default)
    {
        var invoiceItems = createInvoiceRequest.Items
            .Select(item =>
            {
                const decimal defaultVatPercentage = 21.0000M;

                var resultItem = new Item
                {
                    Code = item.Code,
                    Name = item.Name,
                    Unit = item.Unit,
                    UnitPrice = item.UnitPrice.RoundTo(4),
                    Quantity = item.Quantity.RoundTo(2),
                    VatPercentage = item.Vat?.Rate.RoundTo(4) ?? defaultVatPercentage,
                    VatExemptionReason = item.Vat?.ExemptionReason
                };

                resultItem.UnitPriceAfterVat = resultItem.UnitPrice.IncreaseBy(resultItem.VatPercentage).RoundTo(4);
                resultItem.TotalPriceBeforeVat = (resultItem.UnitPrice * resultItem.Quantity).RoundTo(4);
                resultItem.TotalPriceAfterVat = (resultItem.UnitPriceAfterVat * resultItem.Quantity).RoundTo(4);
                resultItem.TotalVatAmount = (resultItem.TotalPriceAfterVat - resultItem.TotalPriceBeforeVat).RoundTo(4);

                return resultItem;
            })
            .ToList();

        var totalPrice = invoiceItems.Sum(i => i.TotalPriceAfterVat).RoundTo(2);
        var totalVatAmount = invoiceItems.Sum(i => i.TotalVatAmount).RoundTo(2);
        var totalPriceWithoutVat = invoiceItems.Sum(i => i.TotalPriceBeforeVat).RoundTo(2);

        var currentDateTime = DateTime.UtcNow.WithoutMilliseconds();

        using var certificate = provideCertificate();

        var (iicSignatureText, iicHashText) = ComputeIic(
            createInvoiceRequest.OrderNumber, currentDateTime, totalPrice, certificate);

        const string defaultRequestId = "Request";
        const int defaultRequestVersion = 1;

        var request = new FiscalizationRequest
        {
            Id = defaultRequestId,
            Version = defaultRequestVersion,
            Header = new FiscalizationHeader
            {
                Uuid = Guid.NewGuid(),
                SentAt = currentDateTime
            },
            Invoice = new Invoice
            {
                Type = createInvoiceRequest.Type,
                TypeOfInvoice = TypeOfInvoice.NonCash,
                IssuedAt = currentDateTime,
                Number = string.Join("/",
                    settings.IssuerBusinessUnitCode,
                    createInvoiceRequest.OrderNumber,
                    createInvoiceRequest.TaxDate.Year,
                    settings.IssuerEnuCode),
                OrderNumber = createInvoiceRequest.OrderNumber,
                EnuCode = settings.IssuerEnuCode,
                IsIssuerInVat = true,
                TotalPriceWithoutVat = totalPriceWithoutVat,
                TotalVatAmount = totalVatAmount,
                TotalPrice = totalPrice,
                OperatorCode = settings.IssuerOperatorCode,
                BusinessUnitCode = settings.IssuerBusinessUnitCode,
                SoftwareCode = settings.IssuerSoftwareCode,
                IssuerInvoiceCodeHash = iicHashText,
                IssuerInvoiceCodeSignature = iicSignatureText,
                TaxPeriod = $"{createInvoiceRequest.TaxDate.Month:00}/{createInvoiceRequest.TaxDate.Year}",
                Note = createInvoiceRequest.Note,
                BankNumber = createInvoiceRequest.BankNumber,
                PayDeadline = createInvoiceRequest.PaymentDeadline.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                TotalPriceToPay = Math.Max(totalPrice, 0),
                PaymentMethods =
                [
                    new PaymentMethod
                    {
                        Type = createInvoiceRequest.PaymentMethod,
                        Amount = totalPrice
                    }
                ],
                Seller = new Seller
                {
                    IdType = settings.IssuerIdType,
                    IdNumber = settings.IssuerIdNumber,
                    Name = settings.IssuerName,
                    Address = settings.IssuerAddress,
                    City = settings.IssuerCity,
                    Country = settings.IssuerCountry
                },
                Buyer = createInvoiceRequest.Buyer,
                Items = invoiceItems,
                SameTaxes = invoiceItems
                    .GroupBy(item => new { item.VatPercentage, item.VatExemptionReason })
                    .Select(vatPercentageGroup => new SameTax
                    {
                        VatPercentage = vatPercentageGroup.Key.VatPercentage.RoundTo(2),
                        VatAmount = vatPercentageGroup.Sum(x => x.TotalVatAmount).RoundTo(2),
                        PriceBeforeVat = vatPercentageGroup.Sum(x => x.TotalPriceBeforeVat).RoundTo(2),
                        TotalItems = vatPercentageGroup.Count(),
                        VatExemptionReason = vatPercentageGroup.Key.VatExemptionReason
                    })
                    .ToList(),
                CorrectiveInvoice = createInvoiceRequest is { Type: InvoiceType.Corrective, Original: not null }
                    ? new CorrectiveInvoice
                    {
                        Type = CorrectiveInvoiceType.Corrective,
                        ReferenceId = createInvoiceRequest.Original.Id,
                        IssuedAt = createInvoiceRequest.Original.IssuedAt
                    }
                    : null
            }
        };

        const string defaultXmlNamespaceUri = "https://efi.tax.gov.me/fs/schema";
        var requestXmlDocument = request.ToXmlDocument(defaultXmlNamespaceUri);
        requestXmlDocument.AppendSelfSignature(certificate);

        var envelopedRequestXmlText = requestXmlDocument.ToEnvelopedXmlText();

        using var httpRequest = new StringContent(envelopedRequestXmlText, Encoding.UTF8, "text/xml");
        using var httpResult = await httpClient
            .PostAsync("fs-v1", httpRequest, cancellationToken)
            .ConfigureAwait(false);

        var responseXmlText = await httpResult.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var responseXmlDocument = new XmlDocument();
        responseXmlDocument.LoadXml(responseXmlText);

        var responseBodyElement = responseXmlDocument.DocumentElement!["env:Body"]!;

        if (!httpResult.IsSuccessStatusCode)
            return new CreateInvoiceResult
            {
                IsSuccessful = false,
                EnvelopedRequestXmlText = envelopedRequestXmlText,
                ErrorMessage = responseBodyElement["env:Fault"]!["faultstring"]!.InnerText,
                InvoiceNumber = request.Invoice.Number,
                Iic = iicHashText,
                TotalPrice = totalPrice,
                CreatedAt = currentDateTime
            };

        var urlQueryParams = new Dictionary<string, object>
        {
            ["tin"] = settings.IssuerIdNumber,
            ["iic"] = iicHashText,
            ["crtd"] = currentDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
            ["ord"] = createInvoiceRequest.OrderNumber,
            ["bu"] = settings.IssuerBusinessUnitCode,
            ["cr"] = settings.IssuerEnuCode,
            ["sw"] = settings.IssuerSoftwareCode,
            ["prc"] = totalPrice.ToFormattedString(2)
        };

        var queryString = string.Join("&", urlQueryParams.Select(p => $"{p.Key}={p.Value}"));

        return new CreateInvoiceResult
        {
            IsSuccessful = true,
            EnvelopedRequestXmlText = envelopedRequestXmlText,
            Fic = responseBodyElement["RegisterInvoiceResponse"]!["FIC"]!.InnerText,
            VerificationUrl =
                $"{UriProvider.GetInvoiceVerificationUri(settings.Environment)}ic/#/verify?{queryString}",
            InvoiceNumber = request.Invoice.Number,
            Iic = iicHashText,
            TotalPrice = totalPrice,
            CreatedAt = currentDateTime
        };
    }

    private (string IicSignatureText, string IicHashText) ComputeIic(
        int orderNumber,
        DateTime currentDateTime,
        decimal totalPrice,
        X509Certificate2 certificate)
    {
        var iicPlainText = string.Join("|",
            settings.IssuerIdNumber,
            currentDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
            orderNumber,
            settings.IssuerBusinessUnitCode,
            settings.IssuerEnuCode,
            settings.IssuerSoftwareCode,
            totalPrice.ToFormattedString(2));

        var iicSignatureBytes = certificate
            .GetRSAPrivateKey()!
            .SignData(Encoding.UTF8.GetBytes(iicPlainText), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var iicSignatureText = Convert.ToHexString(iicSignatureBytes);
        var iicHashBytes = MD5.HashData(iicSignatureBytes);
        var iicHashText = Convert.ToHexString(iicHashBytes);

        return (iicSignatureText, iicHashText);
    }
}