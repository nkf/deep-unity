using System;
using System.IO;
using System.Xml.Serialization;

[Serializable]
public class QImitationStorage {
    public readonly String Name;
    public readonly QExperience Experience;


    public QImitationStorage(string name, QExperience experience) {
        Name = name;
        Experience = experience;
    }

    private QImitationStorage() {}

    public void Save(string path) {
        using(var fs = File.Open(path, FileMode.Create)) {
            new XmlSerializer(typeof(QImitationStorage)).Serialize(fs, this);
        }
    }

    public static QImitationStorage Load(string path) {
        using(var fs = File.Open(path, FileMode.Open)) {
            return new XmlSerializer(typeof(QImitationStorage)).Deserialize(fs) as QImitationStorage;
        }
    }
}
