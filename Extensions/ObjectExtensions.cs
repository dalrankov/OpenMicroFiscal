using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace OpenMicroFiscal.Extensions;

internal static class ObjectExtensions
{
    public static XmlDocument ToXmlDocument<T>(
        this T instance,
        string? defaultNamespace = null) where T : class
    {
        using var memoryStream = new MemoryStream();
        using TextWriter streamWriter = new StreamWriter(memoryStream);

        var xmlSerializer = new XmlSerializer(typeof(T), defaultNamespace);
        xmlSerializer.Serialize(streamWriter, instance);

        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(Encoding.UTF8.GetString(memoryStream.ToArray()));

        return xmlDocument;
    }
}