using System.Xml.Serialization;

namespace OpenMicroFiscal.Models;

[XmlType("Buyer")]
public sealed class Buyer
{
    [XmlAttribute("IDType")] public TaxIdType IdType { get; set; }
    [XmlAttribute("IDNum")] public string IdNumber { get; set; }
    [XmlAttribute("Name")] public string Name { get; set; }
    [XmlAttribute("Country")] public string Country { get; set; }
    [XmlAttribute("Town")] public string City { get; set; }
    [XmlAttribute("Address")] public string Address { get; set; }
}