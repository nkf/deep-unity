﻿﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

public class QAIOptionWindow : EditorWindow {
    private bool _init = false;
	private bool _imitation;
	private bool _learning;
    private bool _remake;
	private bool _show = true;
    private bool _showScenes = false;
    private bool _testing;
    private int _term;

    private List<QStory> _stories = new List<QStory>();
    private string[] _sceneList;

        // Add menu named "My Window" to the Window menu
	[MenuItem ("QAI/Options")]
	static void Init () {
		// Get existing open window or if none, make a new one:
		var window = (QAIOptionWindow)GetWindow(typeof (QAIOptionWindow));
		var ais = FindObjectsOfType<QAI>();

		window._imitation = ais.All(q => q.Imitating);
		window._learning = ais.All(q => q.Learning);
        window._remake = ais.All(q => q.Remake);
        window._term = ais.First().Terminator;

        window._sceneList = window.GetScenes().ToArray();
        window._init = true;
		window.Show();
	}

    private void OnGUI() {
        var ais = FindObjectsOfType<QAI>();
        EditorGUILayout.HelpBox(
            "To train the AI, turn on learning. You can leave this on during play to have the AI adapt over time to the way the user is playing",
            MessageType.None);
        _learning = EditorGUILayout.ToggleLeft("Learning", _learning);
        if (_learning) {
            //PROGRESS BAR
            if (ais.Length > 0) {
                var r = EditorGUILayout.BeginVertical();
                var i = ais.First().Iteration;
                if (i > 0) {
                    EditorGUI.ProgressBar(r, i/(float) _term, "Progress");
                    GUILayout.Space(18);
                }
                EditorGUILayout.EndVertical();
            }

            // REMAKE MODEL
            _remake = EditorGUILayout.ToggleLeft("Remake model", _remake);

            //EPISODE COUNT
            _term = EditorGUILayout.IntField("Terminate after # episodes", _term);

            //IMITATION LEARNING
            if (_show = EditorGUILayout.Foldout(_show, "Imitation Learning")) {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox(
                    "It is possible for the developer to teach the AI the first steps of how to play the game. Implement the method GetImitationAction to send input to the AI and QAI.Imitate to tell the AI that new input is available.",
                    MessageType.Info);
                _imitation = EditorGUILayout.Toggle("Learn from player input", _imitation);
                EditorGUI.indentLevel--;
            }
        }
        if (!_learning) {
            //TESTER
	        if (GUILayout.Button("Run Tester", new GUIStyle(GUI.skin.button) {margin = new RectOffset(50, 50, 0, 0)})) {
	            _testing = true;

	            //EditorApplication.playmodeStateChanged += PlayModeChanged;
                EditorApplication.isPlaying = !EditorApplication.isPlaying;
	        }
        }

        if (_showScenes = EditorGUILayout.Foldout(_showScenes, "Training Scenes")) {
	        EditorGUI.indentLevel++;
            for (int i = 0; i < _stories.Count; i++) {
                var story = _stories[i];
                var index = _sceneList.ToList().IndexOf(story.ScenePath);
                var r = EditorGUILayout.BeginVertical();
                //Scene selection
                GUILayout.Space(15);
                index = EditorGUI.Popup(r, " Scene:", index == -1 ? 0 : index, _sceneList);
                story.ScenePath = _sceneList[index];
                EditorGUILayout.EndVertical();
                //Iteration field
                story.Iterations = EditorGUILayout.IntField("Iterations:", story.Iterations);

                //Index buttons
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label("Change index");
                if (GUILayout.Button("^")) {
                    _stories.Swap(i - 1, i);
                }
                if (GUILayout.Button("v")) {
                    _stories.Swap(i + 1, i);
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

		foreach(var ai in FindObjectsOfType<QAI>()) {
			ai.Imitating = _imitation;
			ai.Learning = _learning;
            ai.Remake = _remake;
            ai.Terminator = _term;
		    ai.Testing = _testing;
		}
	}

    public void DrawImitationSelectionUI(QStory story) {
        EditorGUILayout.BeginHorizontal();
        

        EditorGUILayout.EndHorizontal();
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

	void OnInspectorUpdate() {
		Repaint();
	}

}

