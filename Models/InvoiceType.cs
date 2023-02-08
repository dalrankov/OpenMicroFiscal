using System.Xml.Serialization;

namespace OpenMicroFiscal.Models;

internal enum InvoiceType
{
    [XmlEnum("INVOICE")] Invoice
}