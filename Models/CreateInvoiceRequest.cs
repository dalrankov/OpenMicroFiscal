namespace OpenMicroFiscal.Models;

public sealed class CreateInvoiceRequest
{
    public InvoiceType Type { get; set; }
    public int OrderNumber { get; set; }
    public PaymentMethodType PaymentMethod { get; set; }
    public DateTime PaymentDeadline { get; set; }
    public Buyer Buyer { get; set; }
    public IEnumerable<InvoiceItem> Items { get; set; }
    public string? Note { get; set; }
    public OriginalInvoice? Original { get; set; }
    
    public sealed class OriginalInvoice
    {
        public string Id { get; set; }
        public DateTime IssuedAt { get; set; }
    }
}