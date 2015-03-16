using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;

public class QImitation {
    private readonly QExperience _experience = new QExperience();
    public bool Imitate(QAgent agent) {
        var s = agent.GetState();
        var a = agent.ConvertImitationAction();
        a.Invoke();
        var s0 = agent.GetState();
        var r = s0.Reward;
        _experience.Learn( new SARS {Action = a, State = s, NextState = s0, Reward = r} );
        return s0.IsTerminal;
    }

    public void Save() {
        Directory.CreateDirectory("QData/Imitation");
        var scene = EditorApplication.currentScene;
        scene = Path.GetDirectoryName(scene).Replace(Path.DirectorySeparatorChar.ToString(), "_") +"_"+ Path.GetFileNameWithoutExtension(scene);
        var id = nextSaveId("QData", scene);
        _experience.Save( Path.Combine("QData/Imitation", scene+"-"+id+".xml") );
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

}
