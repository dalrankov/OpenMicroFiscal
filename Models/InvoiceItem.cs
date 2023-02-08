namespace OpenMicroFiscal.Models;

public sealed class InvoiceItem
{
    public string Name { get; set; }
    public string Unit { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal? VatPercentage { get; set; }
}