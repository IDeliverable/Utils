using System;
using System.Xml.Linq;

namespace IDeliverable.Utils.Core
{
    public static class XmlExtensions
    {
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
    }
}