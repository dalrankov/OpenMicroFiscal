using System.Xml.Serialization;

namespace OpenMicroFiscal.Models;

public enum PaymentMethodType
{
    [XmlEnum("ACCOUNT")] BankTransfer,
    [XmlEnum("BUSINESSCARD")] BusinessBankCard
}