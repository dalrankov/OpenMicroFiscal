namespace OpenMicroFiscal.Models;

public sealed class InvoiceItem
{
    public string? Code { get; set; }
    public string Name { get; set; }
    public string Unit { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; }
    public VatSpec? Vat { get; set; }
    
    public sealed class VatSpec
    {
        public decimal Rate { get; set; }
        public string? ExemptionReason { get; set; }
    }
}