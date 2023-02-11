using OpenMicroFiscal.Models;

namespace OpenMicroFiscal;

internal static class UriProvider
{
    private static readonly Uri TestInvoiceFiscalizationUri = new("https://efitest.tax.gov.me/");
    private static readonly Uri ProductionInvoiceFiscalizationUri = new("https://efi.tax.gov.me/");

    private static readonly Uri TestInvoiceVerificationUri = new("https://efitest.tax.gov.me/");
    private static readonly Uri ProductionInvoiceVerificationUri = new("https://mapr.tax.gov.me/");

    public static Uri GetInvoiceFiscalizationUri(FiscalizationEnvironment environment)
    {
        return environment switch
        {
            FiscalizationEnvironment.Test => TestInvoiceFiscalizationUri,
            FiscalizationEnvironment.Production => ProductionInvoiceFiscalizationUri,
            _ => throw new ArgumentOutOfRangeException(nameof(environment))
        };
    }

    public static Uri GetInvoiceVerificationUri(FiscalizationEnvironment environment)
    {
        return environment switch
        {
            FiscalizationEnvironment.Test => TestInvoiceVerificationUri,
            FiscalizationEnvironment.Production => ProductionInvoiceVerificationUri,
            _ => throw new ArgumentOutOfRangeException(nameof(environment))
        };
    }
}