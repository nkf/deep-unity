using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace QAI.Training {
    [Serializable]
    public class QStory {
        public string ScenePath { get; set; }
        public string SceneName {
            get { return Path.GetFileNameWithoutExtension(ScenePath); }
        }
        public int Id { get; private set; }
        public int Iterations { get; set; }
        public List<QImitationStorage> ImitationExperiences = new List<QImitationStorage>();

        public string FilePath { get; private set; }

        public QStory() {
            Id = -1;
        }

        public void Save(string directory, string filename = "Story") {
            Directory.CreateDirectory(directory);
            var serializer = new BinaryFormatter();
            Id = Id == -1 ? NextSaveId(directory, filename) : Id;
            FilePath = Path.Combine(directory, filename + "-" + Id + ".xml");
            using(var fs = File.Open(FilePath, FileMode.Create)) {
                serializer.Serialize(fs, this);
            }
        }

        public void Delete() {
            File.Delete(FilePath);
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
            var serializer = new BinaryFormatter();
            using(var fs = File.Open(path, FileMode.Open)) {
                var qs = (QStory) serializer.Deserialize(fs);
                qs.FilePath = path;
                return qs;
            }
        }

        public static List<QStory> LoadAll(string dir) {
            return Directory.GetFiles(dir, "*.xml").Select(filepath => Load(filepath)).ToList();
        }

        public static List<QStory> LoadForScene(string dir, string scenePath) {
            return LoadAll(dir).Where(s => s.ScenePath == scenePath).ToList();
        }
    }
}
