using System;
using System.IO;
using System.Xml;
using UnityEngine;
using System.Collections;

public static class QData {
    public const string QDataFolder = "QData";
    
    public static void Load<K,V>(string path, SerializableDictionary<K,V> dictionary) {
        path = Path.Combine(QDataFolder, path);
        FileStream fileStream = null;
        XmlReader reader = null;
        try {
            fileStream = File.Open(path, FileMode.Open, FileAccess.Read);
            reader = XmlReader.Create(fileStream);
            dictionary.ReadXml(reader);
            Debug.Log("Loaded QData from: " + path);
        } catch(Exception e) {
            Debug.Log(e);
        } finally {
            if(fileStream != null) fileStream.Close();
            if(reader != null) reader.Close();
        }
    }

    public static void Save<K,V>(string path, SerializableDictionary<K,V> dictionary) {
        path = Path.Combine(QDataFolder, path);
        XmlWriter writer = null;
        var xmlSettings = new XmlWriterSettings() { Indent = true };
        try {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            writer = XmlWriter.Create(File.Open(path, FileMode.Create), xmlSettings); ;
            dictionary.WriteXml(writer);
            Debug.Log("Saved QData at: " + path);
        } catch(Exception e) {
            Debug.Log(e);
        } finally {
            if(writer != null) writer.Close();
        }
    }

    public static string EscapeScenePath(string path) {
        return Path.GetDirectoryName(path).Replace(Path.DirectorySeparatorChar.ToString(), "_") + "_" + Path.GetFileNameWithoutExtension(path);
    }

}
