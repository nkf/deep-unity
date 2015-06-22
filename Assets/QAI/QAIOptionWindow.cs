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
	private bool _initNext = false;
	private bool _remake;
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

	private QAI _manager;

	private GameObject _agent;
	private QTester _tester;

	private QAIMode _mode;

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
		_manager = CreateManager();
		_agent = _manager.ActiveAgent;
		_tester = _manager.Tester;
		_mode = _manager.Mode;
		_remake = _manager.Remake;
		_term = _manager.Terminator;
        _sceneList = GetScenes().ToArray();
        Directory.CreateDirectory(STORY_PATH);
        _stories = QStory.LoadAll(STORY_PATH); // Should read this when serialization works
        _currentStory = _currentStory == null ? null : _stories.Find(s => s.Id == _currentStory.Id);
    }

    private void OnGUI() {
		Init();
		if(_manager == null || _manager != FindObjectOfType<QAI>())
			_init = false;

		_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        EditorApplication.playmodeStateChanged -= PlayModeChange;
        EditorApplication.playmodeStateChanged += PlayModeChange;

        //PROGRESS BAR
		ProgressBar();

		//SETUP
		GUILayout.Space(10);
		EditorGUILayout.LabelField("Setup");
		_agent = (GameObject)EditorGUILayout.ObjectField("Current Agent", _agent, typeof(GameObject), true);
		_tester = (QTester)EditorGUILayout.ObjectField("Current Tester", _tester, typeof(QTester), true);
		_term = EditorGUILayout.IntField("Iterations", _term);
		GUILayout.Space(10);

		//BUTTONS
		Buttons();
		if (GUILayout.Button("Reload all", GUILayout.Height(15))) {
			_init = false;
		}

        //IMITATION LEARNING
		if(_stories.Count == 0) {
			_stories.Add(new QStory());
			_stories[0].Save(STORY_PATH);
		}
        var story = _stories[0];
		GUILayout.Space(20);
		EditorGUILayout.LabelField("Training data");

        //Imitation training
        var r = EditorGUILayout.BeginHorizontal();
        GUILayout.Space(17);
        GUILayout.Label("Imitation learning");
        if (GUILayout.Button("Record")) {
			_mode = QAIMode.Imitating;
            _currentStory = story;
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

        EditorGUILayout.EndScrollView();


		// Set the values in the AI manager to be saved with the scene.
		if(_manager.ActiveAgent != _agent 
		   || _manager.Remake != _remake
		   || _manager.Terminator != _term
		   || _manager.ActiveAgent != _agent
		   || _manager.Tester != _tester
		   || _manager.Mode != _mode) {
			Debug.Log ("Creating undo operation");
			EditorApplication.MarkSceneDirty();
		}
		_manager.Remake = _remake;
		_manager.Terminator = _term;
		_manager.ActiveAgent = _agent;
		_manager.Tester = _tester;

		_manager.Mode = _mode;

        if (_forceStart) {
            _forceStart = false;
            ChangePlayMode();
        }
    }

	private void ProgressBar() {
		var itt = _manager.Iteration;
		if (itt > 0) {
			var r = EditorGUILayout.BeginVertical();
			EditorGUI.ProgressBar(r, itt/(float) _term, "Progress ("+itt+"/"+_term+")");
			GUILayout.Space(18);
			EditorGUILayout.EndVertical();
		}
	}

	private void Buttons() {
		if (!_learnAllStories) {
			GUI.backgroundColor = Color.green;
			var start = GUILayout.Button("Start learning");
			GUI.backgroundColor = Color.white;
			var remake = GUILayout.Button ("Remake and learn");

			if((start || remake) && _agent == null) {
				EditorUtility.DisplayDialog("QAI", "No agent is currently set. Unable to start training.", "OK");
			} 

			_remake = remake && _agent != null;
			if((start || remake) && _agent != null) {
				_mode = QAIMode.Learning;
				_learnAllStories = true;
				ChangePlayMode();
			}

			//TESTER
			var testButton = GUILayout.Button("Run Tester");
			if(testButton && _manager.Tester != null) {
				_mode = QAIMode.Testing;
				ChangePlayMode();
			} else if(testButton && _manager.Tester == null) {
				EditorUtility.DisplayDialog("QAI", "No tester is set. Please create a testing manager and assign it in the editor.", "OK");
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
	}
	
	private void ChangePlayMode() {
        if (!EditorApplication.isPlaying) {
            _starting = true;
		}
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
				Debug.Log ("Saving imiation data");
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
			_init = false;
			Init();

			_mode = QAIMode.Runnning;
			_currentStory = null;
			_initNext = true;

            _learningStory++;
            if (_learnAllStories && _learningStory < _stories.Count) {
                Debug.Log("Starting next learning story");
                LoadNextStory();
                _remake = false;
                _forceStart = true;
				_mode = QAIMode.Learning;
            }
            else {
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
    }

	private QAI CreateManager() {
		var managers = FindObjectsOfType<QAI>();
		if(managers.Length > 1) {
			Debug.Log ("[QAI] - More than one manager detected...");
		}

		var manager = managers.FirstOrDefault();
		if(managers.Length == 0) {
			manager = new GameObject("AI Manager", typeof(QAI)).GetComponent<QAI>();
		}
		return manager;
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