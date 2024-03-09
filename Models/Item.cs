using System.Xml.Serialization;

namespace OpenMicroFiscal.Models;

[XmlType("I")]
public sealed class Item
{
    [XmlAttribute("N")] public string Name { get; set; }
    [XmlAttribute("U")] public string Unit { get; set; }
    [XmlAttribute("UPB")] public decimal UnitPrice { get; set; }
    [XmlAttribute("Q")] public decimal Quantity { get; set; }
    [XmlAttribute("UPA")] public decimal UnitPriceAfterVat { get; set; }
    [XmlAttribute("PB")] public decimal TotalPriceBeforeVat { get; set; }
    [XmlAttribute("PA")] public decimal TotalPriceAfterVat { get; set; }
    [XmlAttribute("VR")] public decimal VatPercentage { get; set; }
    [XmlAttribute("VA")] public decimal TotalVatAmount { get; set; }
    [XmlAttribute("EX")] public string? VatExemptionReason { get; set; }

    public bool ShouldSerializeVatPercentage() => VatPercentage > 0;
}