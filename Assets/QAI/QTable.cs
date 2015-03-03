using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

namespace Assets.QAI {
    class QTable {
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
            var fileStream = File.Open(path, FileMode.OpenOrCreate);
            var reader = XmlReader.Create(fileStream);
            try {
                _table.ReadXml(reader);
            } catch(Exception e) {
                Debug.Log(e);
            } finally {
                fileStream.Close();
                reader.Close();
            }
        }

        public void Save(string path) {
            XmlWriter writer = null;
            var xmlSettings = new XmlWriterSettings() {Indent = true};
            try {
                writer = XmlWriter.Create(File.Open(path, FileMode.Create), xmlSettings); ;
                _table.WriteXml(writer);
            } catch(Exception e) {
                Debug.Log(e);
            } finally {
                if(writer != null) writer.Close();
            }
        }
    }
}
