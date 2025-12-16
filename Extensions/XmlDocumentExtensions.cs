using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Linq;
#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.

namespace OpenMicroFiscal.Extensions;

internal static class XmlDocumentExtensions
{
    public static void AppendSelfSignature(
        this XmlDocument xmlDocument,
        X509Certificate2 certificate)
    {
        var selfSignatureXmlElement = xmlDocument.Sign(certificate);
        xmlDocument.DocumentElement!.AppendChild(selfSignatureXmlElement);
    }

    private static XmlElement Sign(
        this XmlDocument xmlDocument,
        X509Certificate2 certificate)
    {
        var keyInfo = new KeyInfo();
        keyInfo.AddClause(new KeyInfoX509Data(certificate));

        const string referenceUri = "#Request";
        var reference = new Reference {Uri = referenceUri};
        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
        reference.AddTransform(new XmlDsigExcC14NTransform());

        var signedXml = new SignedXml(xmlDocument)
        {
            SigningKey = certificate.GetRSAPrivateKey(),
            SignedInfo =
            {
                CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl,
                SignatureMethod = SignedXml.XmlDsigRSASHA256Url
            },
            KeyInfo = keyInfo
        };

        signedXml.AddReference(reference);
        signedXml.ComputeSignature();

        return signedXml.GetXml();
    }

    public static string ToEnvelopedXmlText(this XmlDocument xmlDocument)
    {
        const string envelopeNamespaceUri = "http://schemas.xmlsoap.org/soap/envelope/";
        const string envelopeElementName = "Envelope";
        const string soapEnvelopeAttributeName = "soapenv";
        const string headerElementName = "Header";
        const string bodyElementName = "Body";

        var envelopeNamespace = (XNamespace) envelopeNamespaceUri;

        var envelopeElement = new XElement(envelopeNamespace + envelopeElementName,
            new XAttribute(XNamespace.Xmlns + soapEnvelopeAttributeName, envelopeNamespace),
            new XElement(envelopeNamespace + headerElementName),
            new XElement(envelopeNamespace + bodyElementName, XElement.Parse(xmlDocument.OuterXml)));

        return envelopeElement.ToString(SaveOptions.DisableFormatting);
    }
}