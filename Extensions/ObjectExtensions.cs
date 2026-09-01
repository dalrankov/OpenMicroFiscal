using System.Xml;
using System.Xml.Serialization;

namespace OpenMicroFiscal.Extensions;

internal static class ObjectExtensions
{
    public static XmlDocument ToXmlDocument<T>(
        this T instance,
        string? defaultNamespace = null) where T : class
    {
        using var stringWriter = new StringWriter();

        var xmlSerializer = new XmlSerializer(typeof(T), defaultNamespace);
        xmlSerializer.Serialize(stringWriter, instance);

        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(stringWriter.ToString());

        return xmlDocument;
    }
}