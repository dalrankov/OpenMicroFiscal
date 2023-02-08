using System.Xml.Serialization;

namespace OpenMicroFiscal.Models;

[XmlType("Header")]
public sealed class FiscalizationHeader
{
    [XmlAttribute("UUID")] public Guid Uuid { get; set; }
    [XmlAttribute("SendDateTime")] public DateTime SentAt { get; set; }
}