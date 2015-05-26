using System.Xml;
using System.Xml.Serialization;

namespace QNetwork {
    static class XmlExtension {
        public static void XmlSerialize<T>(this XmlWriter writer, T obj) {
            new XmlSerializer(obj.GetType()).Serialize(writer, obj);
        }

        public static void XmlDeserialize<T>(this XmlReader reader, ref T obj) {
            obj = (T)new XmlSerializer(obj.GetType()).Deserialize(reader);
        }

        public static T XmlDeserialize<T>(this XmlReader reader) {
            return (T)new XmlSerializer(typeof(T)).Deserialize(reader);
        }
    }
}
