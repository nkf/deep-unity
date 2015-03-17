using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;

public class QImitation {
    public const string ImitationDataPath = "QData/Imitation";
    private readonly QExperience _experience = new QExperience();
    public bool Imitate(QAgent agent) {
        var s = agent.GetState();
        var a = agent.ConvertImitationAction();
        a.Invoke();
        var s0 = agent.GetState();
        var r = s0.Reward;
        _experience.Store( new SARS {Action = a, State = s, NextState = s0, Reward = r} );
        return s0.IsTerminal;
    }

    public void Save() {
        Directory.CreateDirectory("QData/Imitation");
        var scene = EditorApplication.currentScene;
        scene = EscapeScenePath(scene);
        var id = nextSaveId("QData/Imitation", scene);
        _experience.Save( Path.Combine("QData/Imitation", scene+"-"+id+".xml") );
    }

    private static string EscapeScenePath(string path) {
        return Path.GetDirectoryName(path).Replace(Path.DirectorySeparatorChar.ToString(), "_") + "_" + Path.GetFileNameWithoutExtension(path);
    }

    private int nextSaveId(string directory, string prefix) {
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


    public static List<QExperience> GetAllByScene(string scene) {
        return Directory.GetFiles(ImitationDataPath, EscapeScenePath(scene) + "-*").Select(path => QExperience.Load(path)).ToList();
    }

}
