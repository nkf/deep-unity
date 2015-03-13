using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

public class QAIOptionWindow : EditorWindow {
	private bool _imitation;
	private bool _learning;
    private bool _remake;
	private bool _show = true;
    private bool _showScenes = false;
    private int _term;

    private List<QStory> _stories = new List<QStory>();
    private string[] _sceneList;

        // Add menu named "My Window" to the Window menu
	[MenuItem ("QAI/Options")]
	static void Init () {
		// Get existing open window or if none, make a new one:
		QAIOptionWindow window = (QAIOptionWindow)EditorWindow.GetWindow(typeof (QAIOptionWindow));
		var ais = GameObject.FindObjectsOfType<QAI>();

		window._imitation = ais.All(q => q.IMITATING);
		window._learning = ais.All(q => q.LEARNING);
        window._remake = ais.All(q => q.REMAKE);
        window._term = ais.First().TERMINATOR;

        window._sceneList = window.GetScenes().ToArray();

		window.Show();
	}

	void OnGUI() {
		EditorGUILayout.HelpBox("To train the AI, turn on learning. You can leave this on during play to have the AI adapt over time to the way the user is playing", MessageType.None);
		_learning = EditorGUILayout.ToggleLeft("Learning", _learning);
		if(_learning) {
            _remake = EditorGUILayout.ToggleLeft("Remake model", _remake);
            _term = EditorGUILayout.IntField("Terminate after # episodes", _term);
			if(_show = EditorGUILayout.Foldout(_show, "Imitation Learning")) {
				EditorGUI.indentLevel++;
				EditorGUILayout.HelpBox("It is possible for the developer to teach the AI the first steps of how to play the game. Implement the method GetImitationAction to send input to the AI and QAI.Imitate to tell the AI that new input is available.", MessageType.Info);
				_imitation = EditorGUILayout.Toggle("Learn from player input", _imitation);
				EditorGUI.indentLevel--;
			}
		}

	    if (_showScenes = EditorGUILayout.Foldout(_showScenes, "Training Scenes")) {
	        EditorGUI.indentLevel++;
	        foreach (var story in _stories) {
                var index = _sceneList.ToList().IndexOf(story.ScenePath);
                var r = EditorGUILayout.BeginVertical();
                //Scene selection
	            GUILayout.Space(15);
	            index = EditorGUI.Popup(r, "Scene:", index == -1 ? 0 : index, _sceneList);
	            story.ScenePath = _sceneList[index];
	            
                //Iteration field
                story.Iterations = EditorGUILayout.IntField("Iterations:", story.Iterations);
                EditorGUILayout.EndVertical();
                //Index buttons
	            var hr = EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label("Change index");
	            if (GUILayout.Button("^")) {
	                Debug.Log("move up");
	            }
	            if (GUILayout.Button("v")) {
                    Debug.Log("move down");
	            }
                EditorGUILayout.EndHorizontal();
	        }
	        EditorGUI.indentLevel--;
	        var style = new GUIStyle(GUI.skin.button) {margin = new RectOffset(50, 50, 0, 0)};
	        if (GUILayout.Button("New training story", style)) {
	            _stories.Add(new QStory());
	        }
	    }

	    if (GUILayout.Button("LearnIT huehuehue")) {
	        LearnIt();
	    }

		foreach(var ai in GameObject.FindObjectsOfType<QAI>()) {
			ai.IMITATING = _imitation;
			ai.LEARNING = _learning;
            ai.REMAKE = _remake;
            ai.TERMINATOR = _term;
		}
	}

    private IEnumerable<string> GetScenes() {
        var wd = Directory.GetCurrentDirectory();
        var fullPaths = Directory.GetFiles(wd, "*.unity", SearchOption.AllDirectories);
        return fullPaths.Select(p => GetRelativePath(p, wd));
    }

    /// <summary>
    /// http://stackoverflow.com/a/703292
    /// </summary>
    private string GetRelativePath(string filespec, string folder) {
        Uri pathUri = new Uri(filespec);
        // Folders must end in a slash
        if(!folder.EndsWith(Path.DirectorySeparatorChar.ToString())) {
            folder += Path.DirectorySeparatorChar;
        }
        Uri folderUri = new Uri(folder);
        return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString());
    }

    private void LearnIt() {
        EditorApplication.OpenScene(_stories.First().ScenePath);
        EditorApplication.isPlaying = true;
    }
}

