using System.Xml.Serialization;

namespace OpenMicroFiscal.Models;

public enum InvoiceType
{
    [XmlEnum("INVOICE")] Invoice
}