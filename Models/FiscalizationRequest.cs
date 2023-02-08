using System.Xml.Serialization;

namespace OpenMicroFiscal.Models;

[XmlType("RegisterInvoiceRequest")]
internal sealed class FiscalizationRequest
{
    [XmlAttribute("Id")] public string Id { get; set; }
    [XmlAttribute("Version")] public int Version { get; set; }
    [XmlElement("Header")] public FiscalizationHeader Header { get; set; }
    [XmlElement("Invoice")] public Invoice Invoice { get; set; }
}