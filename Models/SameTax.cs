using System.Xml.Serialization;

namespace OpenMicroFiscal.Models;

[XmlType("SameTax")]
public sealed class SameTax
{
    [XmlAttribute("NumOfItems")] public int TotalItems { get; set; }
    [XmlAttribute("PriceBefVAT")] public decimal PriceBeforeVat { get; set; }
    [XmlAttribute("VATRate")] public decimal VatPercentage { get; set; }
    [XmlAttribute("VATAmt")] public decimal VatAmount { get; set; }
}