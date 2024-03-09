using System.Xml.Serialization;

namespace OpenMicroFiscal.Models;

[XmlType("CorrectiveInv")]
public sealed class CorrectiveInvoice
{
    [XmlAttribute("Type")] public CorrectiveInvoiceType Type { get; set; }
    [XmlAttribute("IICRef")] public string ReferenceId { get; set; }
    [XmlAttribute("IssueDateTime")] public DateTime IssuedAt { get; set; }
}