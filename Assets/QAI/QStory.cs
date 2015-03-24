using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class QStory {
    public string ScenePath { get; set; }
    public string SceneName {
        get { return Path.GetFileNameWithoutExtension(ScenePath); }
    }
	public int Id { get; private set; }
    public int Iterations { get; set; }
    public List<QImitationStorage> ImitationExperiences = new List<QImitationStorage>();

	public QStory() {
		Id = -1;
	}

    public void Save(string directory, string filename = "Story") {
		Directory.CreateDirectory(directory);
        var serializer = new XmlSerializer(typeof(QStory));
		Id = Id == -1 ? NextSaveId(directory, filename) : Id;
		using(var fs = File.Open(Path.Combine(directory, filename + "-" + Id + ".xml"), FileMode.Create)) {
            serializer.Serialize(fs, this);
        }
    }

	private int NextSaveId(string directory, string prefix) {
		var files = Directory.GetFiles(directory);
		var idList = new List<int>();
		foreach (var file in files) {
			var name = Path.GetFileNameWithoutExtension(file);
			var split = name.Split('-');
			if(split[0] == prefix) idList.Add(int.Parse(split[1]));
		}
		var id = idList.Count == 0 ? 1 : idList.Max() + 1;
		return id;
	}

    public static QStory Load(string path) {
        var serializer = new XmlSerializer(typeof(QStory));
        using(var fs = File.Open(path, FileMode.Open)) {
            return (QStory) serializer.Deserialize(fs);
        }
    }

	public static List<QStory> LoadAll(string dir) {
		var filePaths = Directory.GetFiles(dir, "*.xml");
		var l = new List<QStory>();
		foreach(var f in filePaths) {
			l.Add(Load (f));
		}
		return l;
	}
}
