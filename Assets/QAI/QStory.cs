using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

public class QStory {
    //[XmlAttribute("ScenePath")]
    public string ScenePath { get; set; }
    public string SceneName {
        get { return Path.GetFileNameWithoutExtension(ScenePath); }
    }
    //[XmlAttribute("Iterations")]
    public int Iterations { get; set; }
    //[XmlElement("ImitationExperiences")]
    public List<QExperience> ImitationExperiences;

    public void Save(string path) {
        var serializer = new XmlSerializer(typeof(QStory));
        using(var fs = File.Open(path, FileMode.Create)) {
            serializer.Serialize(fs, this);
        }
    }

    public static QStory Load(string path) {
        var serializer = new XmlSerializer(typeof(QStory));
        using(var fs = File.Open(path, FileMode.Open)) {
            return (QStory) serializer.Deserialize(fs);
        }
    }
}
