using System.Xml.Serialization;

namespace OpenMicroFiscal.Models;

[XmlType("PayMethod")]
public sealed class PaymentMethod
{
    [XmlAttribute("Type")] public PaymentMethodType Type { get; set; }
    [XmlAttribute("Amt")] public decimal Amount { get; set; }
}