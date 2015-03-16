using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Xml.Serialization;

public class QExperience : IEnumerable<SARS> {
    private List<SARS> _data = new List<SARS>();
    
    private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(List<SARS>));
    public void Save(string path) {
        using (var fs = File.Open(path, FileMode.Create)) {
            Serializer.Serialize(fs, _data);
        }
    }

    public static QExperience Load(string path) {
        using (var fs = File.Open(path, FileMode.Open)) {
            var data = Serializer.Deserialize(fs);
            return new QExperience { _data = data as List<SARS>};
        }
    }

    public void Store(SARS sars) {
        _data.Add(sars);
    }

    public IEnumerator<SARS> GetEnumerator() {
        return _data.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}
