using System.Xml.Serialization;

namespace OpenMicroFiscal.Models;

public enum TypeOfInvoice
{
    [XmlEnum("NONCASH")] NonCash
}