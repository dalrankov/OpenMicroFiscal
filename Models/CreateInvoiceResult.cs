namespace OpenMicroFiscal.Models;

public sealed class CreateInvoiceResult
{
    public bool IsSuccessful { get; set; }
    public string InvoiceNumber { get; set; }
    public string Url { get; set; }
    public string? ErrorResponseText { get; set; }
}