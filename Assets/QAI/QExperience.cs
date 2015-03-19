using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Xml.Serialization;

public class QExperience : IEnumerable<SARS> {
    public string Name;
    private readonly List<SARS> _data = new List<SARS>();
    
    private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(QExperience));
    public void Save(string path) {
        using (var fs = File.Open(path, FileMode.Create)) {
            Serializer.Serialize(fs, this);
        }
    }

    public static QExperience Load(string path) {
        using (var fs = File.Open(path, FileMode.Open)) {
            return (QExperience) Serializer.Deserialize(fs);
        }
    }

    public void Add(SARS sars) {
        _data.Add(sars);
    }

    public IEnumerator<SARS> GetEnumerator() {
        return _data.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

}
