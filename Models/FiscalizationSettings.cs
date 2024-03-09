namespace OpenMicroFiscal.Models;

public sealed class FiscalizationSettings
{
    public FiscalizationEnvironment Environment { get; set; }
    public TaxIdType IssuerIdType { get; set; }
    public string IssuerIdNumber { get; set; }
    public string IssuerName { get; set; }
    public string IssuerAddress { get; set; }
    public string IssuerCity { get; set; }
    public string IssuerCountry { get; set; }
    public string? IssuerBankNumber { get; set; }
    public string IssuerBusinessUnitCode { get; set; }
    public string IssuerSoftwareCode { get; set; }
    public string IssuerOperatorCode { get; set; }
    public string IssuerEnuCode { get; set; }
}