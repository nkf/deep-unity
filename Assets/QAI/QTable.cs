using System;
using System.IO;
using System.Xml;
using UnityEngine;

namespace Assets.QAI {
    public class QTable {
        private readonly SerializableDictionary<QState, SerializableDictionary<QAction, double>> _table;

        public QTable() {
            _table = new SerializableDictionary<QState, SerializableDictionary<QAction, double>>();
        }

        public void Add(QState s, QAction a, double v) {
            SerializableDictionary<QAction, double> qa;
            if(!_table.ContainsKey(s)) {
                qa = new SerializableDictionary<QAction, double>();
                _table.Add(s, qa);
            } else {
                qa = _table[s];
            }
            qa[a] = v;
        }

        public double Query(QState s, QAction a) {
            if(!_table.ContainsKey(s)) return 0;
            var qa = _table[s];
            if(!qa.ContainsKey(a)) return 0;
            return qa[a];
        }

        public void Load(string path) {
            FileStream fileStream = null;
            XmlReader reader = null;
            try {
                fileStream = File.Open(path, FileMode.OpenOrCreate);
                reader = XmlReader.Create(fileStream);
                _table.ReadXml(reader);
                Debug.Log("Loaded QTable " + path);
            } catch(Exception e) {
                Debug.Log(e);
            } finally {
                if (fileStream != null) fileStream.Close();
                if (reader != null) reader.Close();
            }
        }

        public void Save(string path) {
            XmlWriter writer = null;
            var xmlSettings = new XmlWriterSettings() {Indent = true};
            try {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                writer = XmlWriter.Create(File.Open(path, FileMode.Create), xmlSettings); ;
                _table.WriteXml(writer);
                Debug.Log("Saved QTable " + path);
            } catch(Exception e) {
                Debug.Log(e);
            } finally {
                if(writer != null) writer.Close();
            }
        }
    }
}
