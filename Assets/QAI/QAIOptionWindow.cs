using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using Object = UnityEngine.Object;

public class QAIOptionWindow : EditorWindow {
    private const string STORY_PATH = "QData/Story";
    [NonSerialized] private bool _init = false;
    private bool _imitation;
    private bool _learning;
    private bool _remake;
    private bool _showScenes = false;
    private bool _testing;
    private int _term;

    private bool _starting = false;
    private bool _started = false;
    private bool _forceStart = false;
    private Vector2 _scrollPosition;

    private List<QStory> _stories;
    private string[] _sceneList;
    private QStory _currentStory;
    private bool _learnAllStories;
    private int _learningStory;

    // Add menu named "My Window" to the Window menu
    [MenuItem("QAI/Options")]
    private static void OpenWindow() {
        // Get existing open window or if none, make a new one:
        var window = (QAIOptionWindow) GetWindow(typeof (QAIOptionWindow));
        window.Show();
    }

    private void Init() {
        if (_init) return;
        _init = true;

        Debug.Log("Initializing window");
        var ais = FindObjectsOfType<QAI>();
        _imitation = ais.All(q => q.Imitating);
        _learning = ais.All(q => q.Learning);
        _remake = ais.All(q => q.Remake);
        _term = ais.First().Terminator;
        _sceneList = GetScenes().ToArray();
        Directory.CreateDirectory(STORY_PATH);
        _stories = QStory.LoadAll(STORY_PATH); // Should read this when serialization works
        _currentStory = _currentStory == null ? null : _stories.Find(s => s.Id == _currentStory.Id);
    }

    private void OnGUI() {
        Init();
        if (GUILayout.Button("Reload all", GUILayout.Height(15))) {
            _init = false;
        }
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        EditorApplication.playmodeStateChanged -= PlayModeChange;
        EditorApplication.playmodeStateChanged += PlayModeChange;

		EditorGUILayout.Toggle(_learning);
		EditorGUILayout.Toggle(_remake);

        var ais = FindObjectsOfType<QAI>();
        //PROGRESS BAR
        if (ais.Length > 0) {
            var r = EditorGUILayout.BeginVertical();
            var i = ais.First().Iteration;
            if (i > 0) {
                EditorGUI.ProgressBar(r, i/(float) _term, "Progress ("+i+"/"+_term+")");
                GUILayout.Space(18);
            }
            EditorGUILayout.EndVertical();
        }

		if (!_learnAllStories) {
			GUI.backgroundColor = Color.green;
			if (GUILayout.Button("Start learning")) {
				_learning = true;
				_learnAllStories = true;
				ChangePlayMode();
			}
			GUI.backgroundColor = Color.white;
			if (GUILayout.Button ("Remake and learn")) {
				_remake = true;
				_learning = true;
				_learnAllStories = true;
				ChangePlayMode ();
			}
		}
		else {
			var c = GUI.color;
			var bc = GUI.backgroundColor;
			GUI.color = Color.white;
			GUI.backgroundColor = Color.red;
			if (GUILayout.Button("ABORT!")) {
				_learnAllStories = false;
				ChangePlayMode();
			}
			GUI.color = c;
			GUI.backgroundColor = bc;
		}
		
		//TESTER
		if(GUILayout.Button("Run Tester")) {
			_testing = true;
			_learning = false;
			ChangePlayMode();
		}

        //IMITATION LEARNING
        for (int i = 0; i < _stories.Count; i++) {
            var story = _stories[i];
            var dr = EditorGUILayout.BeginHorizontal();
            dr.height = 16;
			dr.x += 4;
			dr.width -= 2;
            GUI.Label(dr, "Story " + story.Id);
            var w = dr.width;
            dr.width = 20;
            dr.x = w - 23;
            if (GUI.Button(dr, "X")) {
                story.Delete();
                _init = false;
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(20);

            var index = Array.IndexOf(_sceneList, story.ScenePath);
            var r = EditorGUILayout.BeginVertical();
            //Scene selection
            GUILayout.Space(15);
			r.x += 1;
			r.width -= 5;
            index = EditorGUI.Popup(r, " Scene:", index == -1 ? 0 : index, _sceneList);
            if (_sceneList[index] != story.ScenePath) {
                story.ImitationExperiences.Clear(); // Huehuehue
                story.ScenePath = _sceneList[index];
                story.Save(STORY_PATH);
            }
            EditorGUILayout.EndVertical();
            //Iteration field
            var itt = EditorGUILayout.IntField("Iterations:", story.Iterations);
            if (itt != story.Iterations) {
                story.Iterations = itt;
                story.Save(STORY_PATH);
            }

            //Imitation training
            r = EditorGUILayout.BeginHorizontal();
            GUILayout.Space(17);
            GUILayout.Label("Imitation learning");
            if (GUILayout.Button("Record")) {
				_learning = true;
                _imitation = true;
                _currentStory = story;
                EditorApplication.OpenScene(story.ScenePath);
                ChangePlayMode();
            }
            EditorGUILayout.EndHorizontal();
            foreach (var exp in story.ImitationExperiences) {
                r = EditorGUILayout.BeginHorizontal();
                GUILayout.Space(35);
                GUILayout.Label(exp.Name);
                if (GUILayout.Button("Delete")) {
                    story.ImitationExperiences.Remove(exp);
                    story.Save(STORY_PATH);
                    return;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        var style = new GUIStyle(GUI.skin.button) {margin = new RectOffset(50, 50, 0, 0)};
        if (GUILayout.Button("New training story", style)) {
            var story = new QStory();
            story.Save(STORY_PATH);
            _stories.Add(story);
        }
        EditorGUILayout.Space();

        EditorGUILayout.EndScrollView();

        foreach (var ai in FindObjectsOfType<QAI>()) {
            ai.Imitating = _imitation;
            ai.Learning = _learning;
            ai.Remake = _remake;
            ai.Terminator = _term;
            ai.Testing = _testing;
        }

        if (_forceStart) {
            _forceStart = false;
            ChangePlayMode();
        }
    }

    private void ChangePlayMode() {
        if (!EditorApplication.isPlaying)
            _starting = true;
        if (_learnAllStories) {
            LoadNextStory();
        }
        EditorApplication.isPlaying = !EditorApplication.isPlaying;
    }

    private void PlayModeChange() {
        if (_started && !EditorApplication.isPlayingOrWillChangePlaymode && EditorApplication.isPlaying) {
            // Stop is called within the playmode scene
            Debug.Log("Stopping in playmode");

            if (_currentStory != null) {
                var imitation = QAI.SaveImitation("Imitation set " + (_currentStory.ImitationExperiences.Count + 1));
                _currentStory.ImitationExperiences.Add(imitation);
                _currentStory.Save(STORY_PATH);
            }
            _currentStory = null;
        }
        if (_started && !EditorApplication.isPlaying) {
            // Stop is called within the editor scene
            Debug.Log("Stopping in editor");
            _started = false;

            // Do something that should happen on stop
            _testing = false;
            _imitation = false;

            _learningStory++;
            if (_learnAllStories && _learningStory < _stories.Count) {
                Debug.Log("Starting next learning story");
                LoadNextStory();
                _remake = false;
                _forceStart = true;
            }
            else {
				_learning = false;
                _learnAllStories = false;
                _learningStory = 0;
				_remake = false;
            }
        }
        if (_starting && EditorApplication.isPlaying) {
            // Start is called within the playmode scene
            Debug.Log("Starting");
            _starting = false;
            _started = true;

            // Do something that should happen on start
        }
    }

    private void LoadNextStory() {
        Debug.Log("Continuing on to next story");
        var story = _stories[_learningStory];
        EditorApplication.OpenScene(story.ScenePath);
        _term = story.Iterations;
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
        if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString())) {
            folder += Path.DirectorySeparatorChar;
        }
        Uri folderUri = new Uri(folder);
        return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString());
    }

    private void OnInspectorUpdate() {
        Repaint();
    }
}