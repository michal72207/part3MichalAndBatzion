using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace GatherUp.Infrastructure.Data
{
    public static class XMLSerializer
    {
        public static void SerializeToXml<T>(string filePath, T data) where T : class
        {
            if (data == null) return;

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(writer, data);
            }
        }

        public static T? DeserializeFromXml<T>(string filePath) where T : class
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            using (StreamReader reader = new StreamReader(filePath))
            {
                var serializer = new XmlSerializer(typeof(T));
                return serializer.Deserialize(reader) as T;
            }
        }
    }

    public static class XMLDocManager
    {
        public static XDocument LoadOrCreateDocument(string filePath, string rootElementName)
        {
            if (File.Exists(filePath))
            {
                return XDocument.Load(filePath);
            }

            return new XDocument(new XElement(rootElementName));
        }

        public static void SaveDocument(XDocument doc, string filePath)
        {
            doc.Save(filePath);
        }
    }
}