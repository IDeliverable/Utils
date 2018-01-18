using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace IDeliverable.Utils.Core
{
    public class Serializer<T>
    {
        public static T DeserializeFromXml(string xml)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(xml))
            {
                var newEntity = (T)serializer.Deserialize(reader);
                return newEntity;
            }
        }

        public static T DeserializeFromStream(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StreamReader(stream))
            {
                var newObject = (T)serializer.Deserialize(reader);
                return newObject;
            }
        }

        public static string SerializeToXml(T obj, bool omitDeclaration = false)
        {
            var builder = new StringBuilder();
            using (var stringWriter = new StringWriter(builder))
            {
                var serializer = new XmlSerializer(typeof(T));
                var settings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = omitDeclaration,
                    Indent = true,
                    IndentChars = "\t"
                };

                // NOTE: The Encoding property cannot be specified when writing 
                // to a StringWriter; UTF-16 will always be used.
                using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
                {
                    serializer.Serialize(xmlWriter, obj);
                    return builder.ToString();
                }
            }
        }

        public static void SerializeToStream(T obj, Stream s, bool omitDeclaration = false)
        {
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = omitDeclaration,
                Indent = true,
                IndentChars = "\t",
                Encoding = Encoding.UTF8
            };

            using (var writer = XmlWriter.Create(s, settings))
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(writer, obj);
            }
        }
    }
}