using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

public class QStory {
    public string ScenePath { get; set; }
    public string SceneName {
        get { return Path.GetFileNameWithoutExtension(ScenePath); }
    }
    public int Iterations { get; set; }
    public List<QImitationStorage> ImitationExperiences;

    public void Save(string path) {
        var serializer = new XmlSerializer(typeof(QStory));
        using(var fs = File.Open(path, FileMode.Create)) {
            serializer.Serialize(fs, this);
        }
    }

    public static QStory Load(string path) {
        var serializer = new XmlSerializer(typeof(QStory));
        using(var fs = File.Open(path, FileMode.Open)) {
            return serializer.Deserialize(fs) as QStory;
        }
    }
}
