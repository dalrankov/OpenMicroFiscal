using System.Xml.Serialization;

namespace OpenMicroFiscal.Models;

[XmlType("Header")]
internal sealed class FiscalizationHeader
{
    [XmlAttribute("UUID")] public Guid Uuid { get; set; }
    [XmlAttribute("SendDateTime")] public DateTime SentAt { get; set; }
}