using System.Xml.Serialization;

namespace OpenMicroFiscal.Models;

public enum TaxIdType
{
    [XmlEnum("TIN")] Tin,
    [XmlEnum("PASS")] Passport,
    [XmlEnum("VAT")] Vat
}