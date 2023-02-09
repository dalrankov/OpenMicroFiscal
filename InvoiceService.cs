using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using OpenMicroFiscal.Extensions;
using OpenMicroFiscal.Models;

namespace OpenMicroFiscal;

public sealed class InvoiceService
{
    private readonly HttpClient _httpClient;
    private readonly ProvideCertificate _provideCertificate;
    private readonly FiscalizationSettings _settings;

    public InvoiceService(
        HttpClient httpClient,
        FiscalizationSettings settings,
        ProvideCertificate provideCertificate)
    {
        _httpClient = httpClient;
        _settings = settings;
        _provideCertificate = provideCertificate;
    }

    public async Task<CreateInvoiceResult> CreateInvoiceAsync(
        CreateInvoiceRequest createInvoiceRequest,
        CancellationToken cancellationToken = default)
    {
        const decimal defaultVatPercentage = 21.0000M;
        
        var invoiceItems = createInvoiceRequest.Items
            .Select(inputItem => new Item
            {
                Name = inputItem.Name,
                Unit = inputItem.Unit,
                UnitPrice = inputItem.UnitPrice.RoundTo(4),
                Quantity = inputItem.Quantity,
                VatPercentage = inputItem.VatPercentage?.RoundTo(4) ?? defaultVatPercentage
            })
            .Select(resultItem =>
            {
                resultItem.UnitPriceAfterVat = resultItem.UnitPrice.IncreaseBy(resultItem.VatPercentage).RoundTo(4);
                resultItem.TotalPriceBeforeVat = resultItem.UnitPrice * resultItem.Quantity;
                resultItem.TotalPriceAfterVat = resultItem.UnitPriceAfterVat * resultItem.Quantity;
                resultItem.TotalVatAmount = resultItem.TotalPriceAfterVat - resultItem.TotalPriceBeforeVat;

                return resultItem;
            })
            .ToList();

        var totalPrice = invoiceItems.Sum(i => i.TotalPriceAfterVat).RoundTo(2);
        var totalVatAmount = invoiceItems.Sum(i => i.TotalVatAmount).RoundTo(2);
        var totalPriceWithoutVat = invoiceItems.Sum(i => i.TotalPriceBeforeVat).RoundTo(2);

        var currentDateTime = DateTime.UtcNow.WithoutSeconds();

        using var certificate = _provideCertificate();

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
                Type = InvoiceType.Invoice,
                TypeOfInvoice = TypeOfInvoice.NonCash,
                IssuedAt = currentDateTime,
                Number = string.Join("/",
                    _settings.IssuerBusinessUnitCode,
                    createInvoiceRequest.OrderNumber,
                    currentDateTime.Year,
                    _settings.IssuerEnuCode),
                OrderNumber = createInvoiceRequest.OrderNumber,
                EnuCode = _settings.IssuerEnuCode,
                IsIssuerInVat = true,
                TotalPriceWithoutVat = totalPriceWithoutVat,
                TotalVatAmount = totalVatAmount,
                TotalPrice = totalPrice,
                OperatorCode = _settings.IssuerOperatorCode,
                BusinessUnitCode = _settings.IssuerBusinessUnitCode,
                SoftwareCode = _settings.IssuerSoftwareCode,
                IssuerInvoiceCodeHash = iicHashText,
                IssuerInvoiceCodeSignature = iicSignatureText,
                TaxPeriod = $"{currentDateTime.Month:00}/{currentDateTime.Year}",
                Note = createInvoiceRequest.Note,
                BankNumber = _settings.IssuerBankNumber,
                PayDeadline = createInvoiceRequest.PaymentDeadline.ToString("yyyy-MM-dd"),
                TotalPriceToPay = totalPrice,
                TaxFreeAmount = totalPrice - totalVatAmount,
                PaymentMethods = new List<PaymentMethod>
                {
                    new()
                    {
                        Type = createInvoiceRequest.PaymentMethod,
                        Amount = totalPrice
                    }
                },
                Seller = new Seller
                {
                    IdType = _settings.IssuerIdType,
                    IdNumber = _settings.IssuerIdNumber,
                    Name = _settings.IssuerName,
                    Address = _settings.IssuerAddress,
                    City = _settings.IssuerCity,
                    Country = _settings.IssuerCountry
                },
                Buyer = createInvoiceRequest.Buyer,
                Items = invoiceItems,
                SameTaxes = invoiceItems
                    .GroupBy(item => item.VatPercentage)
                    .Select(vatPercentageGroup => new SameTax
                    {
                        VatPercentage = vatPercentageGroup.Key.RoundTo(2),
                        VatAmount = vatPercentageGroup.Sum(x => x.TotalVatAmount).RoundTo(2),
                        PriceBeforeVat = vatPercentageGroup.Sum(x => x.TotalPriceBeforeVat).RoundTo(2),
                        TotalItems = vatPercentageGroup.Sum(x => x.Quantity)
                    })
                    .ToList()
            }
        };

        const string defaultXmlNamespaceUri = "https://efi.tax.gov.me/fs/schema";
        var requestXmlDocument = request.ToXmlDocument(defaultXmlNamespaceUri);
        requestXmlDocument.AppendSelfSignature(certificate);

        var envelopedRequestXmlText = requestXmlDocument.ToEnvelopedXmlText();

        using var httpRequest = new StringContent(envelopedRequestXmlText, Encoding.UTF8, "text/xml");
        using var httpResult = await _httpClient
            .PostAsync("fs-v1", httpRequest, cancellationToken)
            .ConfigureAwait(false);

        var responseXmlText = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        var responseXmlDocument = new XmlDocument();
        responseXmlDocument.LoadXml(responseXmlText);

        var responseBodyElement = responseXmlDocument.DocumentElement!["env:Body"]!;
        
        if (!httpResult.IsSuccessStatusCode)
            return new CreateInvoiceResult
            {
                IsSuccessful = false,
                ErrorMessage = responseBodyElement["env:Fault"]!["faultstring"]!.InnerText,
                InvoiceNumber = request.Invoice.Number,
                Iic = iicHashText,
                TotalPrice = totalPrice,
                CreatedAt = currentDateTime
            };

        var urlQueryParams = new Dictionary<string, object>
        {
            ["tin"] = _settings.IssuerIdNumber,
            ["iic"] = iicHashText,
            ["crtd"] = currentDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            ["ord"] = createInvoiceRequest.OrderNumber,
            ["bu"] = _settings.IssuerBusinessUnitCode,
            ["cr"] = _settings.IssuerEnuCode,
            ["sw"] = _settings.IssuerSoftwareCode,
            ["prc"] = totalPrice.ToString("0.00", CultureInfo.InvariantCulture)
        };

        var queryString = string.Join("&", urlQueryParams.Select(p => $"{p.Key}={p.Value}"));

        return new CreateInvoiceResult
        {
            IsSuccessful = true,
            Fic = responseBodyElement["RegisterInvoiceResponse"]!["FIC"]!.InnerText,
            VerificationUrl = $"{UriProvider.GetInvoiceVerificationUri(_settings.Environment)}ic/#/verify?{queryString}",
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
            _settings.IssuerIdNumber,
            currentDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            orderNumber,
            _settings.IssuerBusinessUnitCode,
            _settings.IssuerEnuCode,
            _settings.IssuerSoftwareCode,
            totalPrice.ToString("0.00", CultureInfo.InvariantCulture));

        var iicSignatureBytes = certificate
            .GetRSAPrivateKey()!
            .SignData(Encoding.UTF8.GetBytes(iicPlainText), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var iicSignatureText = BitConverter.ToString(iicSignatureBytes).Replace("-", string.Empty);

        using var md5 = MD5.Create();
        var iicHashBytes = md5.ComputeHash(iicSignatureBytes);
        var iicHashText = BitConverter.ToString(iicHashBytes).Replace("-", string.Empty);

        return (iicSignatureText, iicHashText);
    }
}