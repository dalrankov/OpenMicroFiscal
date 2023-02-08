namespace OpenMicroFiscal.Models;

public sealed class CreateInvoiceRequest
{
    public int OrderNumber { get; set; }
    public PaymentMethodType PaymentMethod { get; set; }
    public DateTime PaymentDeadline { get; set; }
    public Buyer Buyer { get; set; }
    public IEnumerable<InvoiceItem> Items { get; set; }
    public string? Note { get; set; }
}