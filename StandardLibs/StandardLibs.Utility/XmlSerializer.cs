using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace StandardLibs.Utility
{
    public class XmlSerializer<T> : ISerializer<T>
    {
        private static readonly XmlWriterSettings WriterSettings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true, Encoding = new UTF8Encoding(false) };
        private XmlSerializer serializer { get; set; }
        private XmlSerializerNamespaces namespaces { get; set; }

        public XmlSerializer()
        {
            this.namespaces = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", "") });
            this.serializer = new XmlSerializer(typeof(T));
        }

        public XmlSerializer(string prefix, string ns)
        {
            this.namespaces = new XmlSerializerNamespaces(new[] { new XmlQualifiedName(prefix, ns) });
            this.serializer = new XmlSerializer(typeof(T));
        }

        public byte[] Serialize2Bytes(T entity)
        {
            if (entity == null)
            {
                return null;
            }
            using (var ms = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(ms, WriterSettings))
                {
                    this.serializer.Serialize(writer, entity, this.namespaces);
                    return ms.ToArray();
                }
            }
        }

        public string Serialize(T entity)
        {
            byte[] result = this.Serialize2Bytes(entity);
            if (result == null)
            {
                return null;
            }
            return Encoding.UTF8.GetString(result);
        }

        public T Deserialize(byte[] serialized)
        {
            return this.Deserialize(Encoding.UTF8.GetString(serialized));
        }

        public T Deserialize(string serialized)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(serialized);
            XmlNodeReader reader = new XmlNodeReader(xdoc.DocumentElement);
            object obj = serializer.Deserialize(reader);
            return (T)obj;
        }
    }
}
