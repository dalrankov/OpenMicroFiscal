using System.Xml.Serialization;

namespace OpenMicroFiscal.Models;

internal enum TypeOfInvoice
{
    [XmlEnum("NONCASH")] NonCash
}