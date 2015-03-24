﻿using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;

public class QImitation {
    public const string ImitationDataPath = "QData/Imitation";
    private readonly QExperience _experience = new QExperience();
    public bool Imitate(QAgent agent) {
        var a = agent.ConvertImitationAction();
        var sars = agent.MakeSARS(a);
        _experience.Store(sars);
        return sars.NextState.IsTerminal;
    }

	public QImitationStorage CreateStorageItem(string id) {
		return new QImitationStorage(id, _experience);
	}

    public void Save() {
		Directory.CreateDirectory("QData/Imitation");
		var scene = EditorApplication.currentScene;
		scene = EscapeScenePath(scene);
		var id = NextSaveId("QData/Imitation", scene);
		CreateStorageItem(id.ToString()).Save(Path.Combine("QData/Imitation", scene + "-" + id + ".xml"));
    }

    private static string EscapeScenePath(string path) {
        return Path.GetDirectoryName(path).Replace(Path.DirectorySeparatorChar.ToString(), "_") + "_" + Path.GetFileNameWithoutExtension(path);
    }

    private int NextSaveId(string directory, string prefix) {
        var files = Directory.GetFiles(directory);
        var idList = new List<int>();
        foreach (var file in files) {
            var name = Path.GetFileNameWithoutExtension(file);
            var split = name.Split('-');
            if(split[0] == prefix) idList.Add(int.Parse(split[1]));
        }
        var id = 0;
        while (idList.Contains(id)) id++;
        return id;
    }


    public static List<QImitationStorage> GetAllByScene(string scene) {
        return Directory.GetFiles(ImitationDataPath, EscapeScenePath(scene) + "-*").Select(path => QImitationStorage.Load(path)).ToList();
    }

}
