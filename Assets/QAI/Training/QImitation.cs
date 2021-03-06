﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using QAI.Agent;
using UnityEditor;

namespace QAI.Training {
    public class QImitation {
        public const string ImitationDataPath = "QData/Imitation";
        private readonly QExperience _experience = new QExperience();
		private QState _prevState;
		private QAction _prevAction;
        public bool Imitate(QState state, QAction a) {
			if(_prevState.Features.Spatial != null) {
				var sars = new SARS (_prevState, _prevAction, state);
	            if(!sars.State.Equals(sars.NextState))
	                _experience.Store(sars);
			}

			a.Invoke();
			_prevState = state;
			_prevAction = a;

			UnityEngine.Debug.Log(_experience.Count);
            return state.IsTerminal;
        }
        //Take the current state and set it as the last's state "next state"
        private void ModLastExp(QState state) {
            var c = _experience.Count;
            if(c > 0) {
                var sars = _experience[c - 1];
                _experience[c - 1] = new SARS(sars.State, sars.Action, state);
            }
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
            foreach(var file in files) {
                var name = Path.GetFileNameWithoutExtension(file);
                var split = name.Split('-');
                if(split[0] == prefix) idList.Add(int.Parse(split[1]));
            }
            var id = 0;
            while(idList.Contains(id)) id++;
            return id;
        }


        public static List<QImitationStorage> GetAllByScene(string scene) {
            return Directory.GetFiles(ImitationDataPath, EscapeScenePath(scene) + "-*").Select(path => QImitationStorage.Load(path)).ToList();
        }

    }
}
