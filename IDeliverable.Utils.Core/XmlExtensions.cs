using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace IDeliverable.Utils.Core
{
    public static class XmlExtensions
    {
        public static XmlNamespaceManager CreateNamespaceManager(this XNode xml, string prefix, string uri)
        {
            var nav = xml.CreateNavigator();
            var nsm = new XmlNamespaceManager(nav.NameTable);
            nsm.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance"); // Required to check for xsi:null.
            nsm.AddNamespace(prefix, uri);
            return nsm;
        }
        
        public static string ReadAttribute(this XElement element, string name, Func<string> defaultValueFunc = null)
        {
            var attribute = element.Attribute(name);

            if (attribute == null)
                return defaultValueFunc != null ? defaultValueFunc() : null;

            return attribute.Value;
        }

        public static T ReadAttribute<T>(this XElement element, string name, Func<string, T> readFunc, Func<T> defaultValueFunc = null)
        {
            var value = ReadAttribute(element, name);

            if (String.IsNullOrWhiteSpace(value))
                return defaultValueFunc != null ? defaultValueFunc() : default(T);

            return readFunc(value);
        }

        public static byte[] ToByteArray(this XElement element)
        {
            var settings = new XmlWriterSettings()
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                NamespaceHandling = NamespaceHandling.OmitDuplicates
            };

            using (var s = new MemoryStream())
            { 
                using (var writer = XmlWriter.Create(s, settings))
                    element.Save(writer);

                return s.ToArray();
            }
        }
    }
}