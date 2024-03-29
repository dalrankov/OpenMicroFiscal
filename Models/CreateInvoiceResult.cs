﻿namespace OpenMicroFiscal.Models;

public sealed class CreateInvoiceResult
{
    public bool IsSuccessful { get; set; }
    public string EnvelopedRequestXmlText { get; set; }
    public string InvoiceNumber { get; set; }
    public string Iic { get; set; }
    public string? Fic { get; set; }
    public string? VerificationUrl { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ErrorMessage { get; set; }
}