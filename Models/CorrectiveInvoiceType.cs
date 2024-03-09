using System.Xml.Serialization;

namespace OpenMicroFiscal.Models;

public enum CorrectiveInvoiceType
{
    [XmlEnum("CORRECTIVE")] Corrective
}