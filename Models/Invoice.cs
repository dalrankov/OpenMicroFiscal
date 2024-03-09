using System.Xml.Serialization;

namespace OpenMicroFiscal.Models;

[XmlType("Invoice")]
public sealed class Invoice
{
    [XmlAttribute("InvType")] public InvoiceType Type { get; set; }
    [XmlAttribute("TypeOfInv")] public TypeOfInvoice TypeOfInvoice { get; set; }
    [XmlAttribute("IssueDateTime")] public DateTime IssuedAt { get; set; }
    [XmlAttribute("InvNum")] public string Number { get; set; }
    [XmlAttribute("InvOrdNum")] public int OrderNumber { get; set; }
    [XmlAttribute("TCRCode")] public string EnuCode { get; set; }
    [XmlAttribute("IsIssuerInVAT")] public bool IsIssuerInVat { get; set; }
    [XmlAttribute("TotPriceWoVAT")] public decimal TotalPriceWithoutVat { get; set; }
    [XmlAttribute("TotVATAmt")] public decimal TotalVatAmount { get; set; }
    [XmlAttribute("TotPrice")] public decimal TotalPrice { get; set; }
    [XmlAttribute("TotPriceToPay")] public decimal TotalPriceToPay { get; set; }
    [XmlAttribute("OperatorCode")] public string OperatorCode { get; set; }
    [XmlAttribute("BusinUnitCode")] public string BusinessUnitCode { get; set; }
    [XmlAttribute("SoftCode")] public string SoftwareCode { get; set; }
    [XmlAttribute("IIC")] public string IssuerInvoiceCodeHash { get; set; }
    [XmlAttribute("IICSignature")] public string IssuerInvoiceCodeSignature { get; set; }
    [XmlAttribute("TaxPeriod")] public string TaxPeriod { get; set; }
    [XmlAttribute("Note")] public string? Note { get; set; }
    [XmlAttribute("BankAccNum")] public string? BankNumber { get; set; }
    [XmlAttribute("PayDeadline")] public string PayDeadline { get; set; }
    [XmlArray("PayMethods")] public List<PaymentMethod> PaymentMethods { get; set; }
    [XmlElement("Seller")] public Seller Seller { get; set; }
    [XmlElement("Buyer")] public Buyer Buyer { get; set; }
    [XmlArray("Items")] public List<Item> Items { get; set; }
    [XmlArray("SameTaxes")] public List<SameTax> SameTaxes { get; set; }
    [XmlElement("CorrectiveInv")] public CorrectiveInvoice? CorrectiveInvoice { get; set; }
}