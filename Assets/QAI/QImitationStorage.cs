using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

public class QImitationStorage {
    private QExperience _experience;
    private String _name;

    private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(QImitationStorage));
    public void Save(string path) {
        using(var fs = File.Open(path, FileMode.Create)) {
            Serializer.Serialize(fs, this);
        }
    }

    public static QImitationStorage Load(string path) {
        using(var fs = File.Open(path, FileMode.Open)) {
            return Serializer.Deserialize(fs) as QImitationStorage;
        }
    }
}
