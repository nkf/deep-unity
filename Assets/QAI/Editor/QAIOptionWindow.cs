using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QAI.Training;
using QAI.Utility;
using UnityEditor;
using UnityEngine;

namespace QAI {
    public class QAIOptionWindow : EditorWindow {
        private const string STORY_PATH = "QData/Story";
        [NonSerialized] private bool _init = false;
        private bool _remake;
        private bool _benchmark;
        private int _term;
		private bool _visualize;

        private bool _starting = false;
        private bool _started = false;
        private bool _forceStart = false;
        private Vector2 _scrollPosition;
		private bool _showAdvanced = false;

        private List<QStory> _stories;
        private QStory _currentStory;
        private bool _learnAllStories;
        private int _learningStory;

        private QAIManager _manager;

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

            _manager = CreateManager();
            _agent = _manager.ActiveAgent;
            _tester = _manager.Tester;
            _mode = _manager.Mode;
            _remake = _manager.Remake;
            _term = _manager.Terminator;
            Directory.CreateDirectory(STORY_PATH);
            _stories = QStory.LoadForScene(STORY_PATH, EditorApplication.currentScene); // Should read this when serialization works
            _currentStory = _currentStory == null ? null : _stories.Find(s => s.Id == _currentStory.Id);
			_benchmark = _manager.Benchmark;
        }

        private void OnGUI() {
            Init();
            if(_manager == null || _manager != FindObjectOfType<QAIManager>())
                _init = false;

			_mode = _manager.ModeOverride;

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            EditorApplication.playmodeStateChanged -= PlayModeChange;
            EditorApplication.playmodeStateChanged += PlayModeChange;

            //PROGRESS BAR
            ProgressBars();

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
                var newStory = new QStory();
                newStory.ScenePath = EditorApplication.currentScene;
                newStory.Save(STORY_PATH);
                _stories.Add(newStory);
            }
            var story = _stories[0];
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Training data");

            //Imitation training
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(17);
            GUILayout.Label("Imitation learning");
            if (GUILayout.Button("Record")) {
                _mode = QAIMode.Imitating;
                _currentStory = story;
                ChangePlayMode();
            }
            EditorGUILayout.EndHorizontal();
            foreach (var exp in story.ImitationExperiences) {
                EditorGUILayout.BeginHorizontal();
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

			if(_showAdvanced = EditorGUILayout.Foldout(_showAdvanced, "Advanced options")) {
				EditorGUI.indentLevel++;
				_visualize = EditorGUILayout.Toggle("Visualize Network", _visualize);
				_manager.PrioritizedSweeping = EditorGUILayout.Toggle ("Priority Sweeping", _manager.PrioritizedSweeping);
			}


            // Set the values in the AI manager to be saved with the scene.
            if(_manager.ActiveAgent != _agent 
               || _manager.Remake != _remake
               || _manager.Terminator != _term
               || _manager.ActiveAgent != _agent
               || _manager.Tester != _tester
               || _manager.Mode != _mode
			   || _manager.VisualizeNetwork != _visualize) {
                EditorApplication.MarkSceneDirty();
            }

            _manager.Remake = _remake;
            _manager.Benchmark = _benchmark;
            _manager.Terminator = _term;
            _manager.ActiveAgent = _agent;
            _manager.Tester = _tester;
			_manager.VisualizeNetwork = _visualize;
			_manager.Mode = _mode;
			_manager.ModeOverride = _mode;

//            _manager.OptionWindow = this;

            if (_forceStart) {
                _forceStart = false;
                ChangePlayMode();
            }
        }

        private void ProgressBars() {
            if (_benchmark) {
                var itt = BenchmarkSave.TestN;
                var total = BenchmarkSave.Runs;
                var r = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(r, itt / (float)total, "Benchmark (" + itt + "/" + total + ")");
                GUILayout.Space(18);
                EditorGUILayout.EndVertical();
            }
            if (_mode == QAIMode.Learning) {
                var itt = QAIManager.Iteration;
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
                var testButton = GUILayout.Button("Run Tester");
                GUILayout.Space(10);
                var benchmark = GUILayout.Button("Begin Benchmark");
                GUILayout.Space(10);

                if (start || remake || benchmark) {
                    if (_agent == null) {
                        EditorUtility.DisplayDialog("QAI", "No agent is currently set. Unable to start training.", "OK");
                    } else {
                        _mode = QAIMode.Learning;
                        _learnAllStories = true;
                        _remake = remake || benchmark;
                        _benchmark = benchmark;
                        if(!benchmark) ChangePlayMode();
						else BenchmarkDialog.OpenWindow(this, _manager);
                    }
                }

                //TESTER
                if(testButton && _manager.Tester != null) {
                    _mode = QAIMode.Testing;
					_manager.BenchmarkID = null;
					if(!Event.current.alt)
						ChangePlayMode();
					else {
						var path = EditorUtility.OpenFilePanel("Open brain", BenchmarkSave.TestFolder, "xml");
						if(path.Equals("")) Reset();
	                    else {
//							var fullFile = path.Substring(path.LastIndexOf(Path.DirectorySeparatorChar)+1);
//							var fileEnd = fullFile.LastIndexOf('-');
//							var filename = fullFile.Substring(0, fileEnd);
							_manager.BenchmarkID = path;
							ChangePlayMode();
						}
					}
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
	
        public void ChangePlayMode() {
            if (!EditorApplication.isPlaying) {
                _starting = true;
            }
            EditorApplication.isPlaying = !EditorApplication.isPlaying;
        }

		public void Reset() {
			Debug.Log ("Resetting");
			_remake = false;
			_learnAllStories = false;
			_benchmark = false;
			_mode = QAIMode.Runnning;
		}

        private void PlayModeChange() {
            if (_started && !EditorApplication.isPlayingOrWillChangePlaymode && EditorApplication.isPlaying) {
                // Stop is called within the playmode scene

                if (_currentStory != null) {
                    Debug.Log ("Saving imiation data");
                    var imitation = QAIManager.SaveImitation("Imitation set " + (_currentStory.ImitationExperiences.Count + 1));
                    _currentStory.ImitationExperiences.Add(imitation);
                    _currentStory.Save(STORY_PATH);
                }
                _currentStory = null;
            }
            if (_started && !EditorApplication.isPlaying) {
                // Stop is called within the editor scene
                _started = false;

                // Do something that should happen on stop
                _init = false;
                Init();

                _mode = QAIMode.Runnning;
				_manager.ModeOverride = _mode;
                _currentStory = null;

                _learningStory++;
                if (_learnAllStories && _learningStory < _stories.Count) {
                    Debug.Log("Starting next learning story");
                    _remake = false;
                    _forceStart = true;
                    _mode = QAIMode.Learning;
                }
                else {
                    _learnAllStories = false;
					_benchmark = false;
                    _learningStory = 0;
                    _remake = false;
                }
            }
            if (_starting && EditorApplication.isPlaying) {
                // Start is called within the playmode scene
                _starting = false;
                _started = true;

                // Do something that should happen on start
            }
        }

        public void SetMode(QAIMode mode) {
            _mode = mode;
        }

        private QAIManager CreateManager() {
            var managers = FindObjectsOfType<QAIManager>();
            if(managers.Length > 1) {
                Debug.Log ("[QAI] - More than one manager detected...");
            }

            var manager = managers.FirstOrDefault();
            if(managers.Length == 0) {
                manager = new GameObject("AI Manager", typeof(QAIManager)).GetComponent<QAIManager>();
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
}