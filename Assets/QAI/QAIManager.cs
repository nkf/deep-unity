﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using QAI.Agent;
using QAI.Learning;
using QAI.Training;
using QAI.Utility;
using QAI.Visualizer;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace QAI {
    public class QAIManager : MonoBehaviour {

        public const float TimeStep = 0.3f;
        [HideInInspector]
		private QAIMode _mode;
        public QAIMode Mode;
        [HideInInspector]
        public bool Remake;
        [HideInInspector] 
        public bool Benchmark;
        [HideInInspector]
        public int Terminator;
		[HideInInspector]
		public bool VisualizeNetwork;

        [HideInInspector]
        public List<QStory> Stories;
        [HideInInspector]
		public QAIMode ModeOverride;
//        public QAIOptionWindow OptionWindow;

		[HideInInspector]
		public string BenchmarkID = "TEST_ID_GOES_HERE";
		[HideInInspector]
		public int BenchmarkRuns = 10;

		[HideInInspector]
		public bool PrioritizedSweeping;
	
        public GameObject ActiveAgent;
        public static int Iteration { get { return _instance == null || _instance._qlearning == null ? 0 : _instance._qlearning.Iteration; }}
        public static QAIMode CurrentMode { get { return _instance.Mode; } }
        public static Action<Vector<float>, bool> NetworkValuesUpdated;
        public static QAgent Agent { get; private set; }

        public QTester Tester;

        private static QAIManager _instance = null;
        private QLearning _qlearning;
        private QImitation _imitation;

        private NetworkVisualizer _visualizer;

        private bool _testIsRunning;
        private bool _testIsOver = false;

        private bool _sceneIsOver;

        private Stopwatch _stopwatch;

        public static int NumIterations() {
            return _instance == null ? 1 : _instance.Terminator;
        }

        public static void InitAgent(QAgent agent, QOption option = null) {
			option = option ?? new QOption();
            if (_instance == null) {
                _instance = FindObjectOfType<QAIManager>();
                _instance.Init(agent, option);
            }
            BenchmarkSave.SaveBenchmarks = _instance.Benchmark;
            _instance._sceneIsOver = false;
            _instance._testIsOver = false;
            Agent = agent;
            if (_instance.Mode != QAIMode.Imitating)
                _instance._qlearning.Reset(agent);
        }

		private void Init(QAgent agent, QOption option) {
			if(Benchmark) {
				BenchmarkSave.CurrentTestID = _instance.BenchmarkID;
				BenchmarkSave.Runs = _instance.BenchmarkRuns;
			} else if(Mode == QAIMode.Testing && BenchmarkID != null && !BenchmarkID.Equals("")) {
				BenchmarkSave.ModelPath = _instance.BenchmarkID;
			} else {
				BenchmarkSave.CurrentTestID = agent.AI_ID().ID;
				BenchmarkSave.Runs = 1;
			}
			Debug.Log ("Running " + BenchmarkSave.ModelPath + " in mode " + Mode);

            _stopwatch = Stopwatch.StartNew();
            if(Tester != null) Tester.Init();

            DontDestroyOnLoad(gameObject);
            switch (Mode) {
                case QAIMode.Imitating: {
                    _imitation = new QImitation();
                    break;
                }
                default: {
					var qlCNN = new QLearningCNN(PrioritizedSweeping, option);
                    _qlearning = qlCNN;
                    _qlearning.Reset(agent);
                    
                    if(Remake) _qlearning.RemakeModel(agent.GetState());
                    else       _qlearning.LoadModel();

                    if(VisualizeNetwork) 
                        _visualizer = _qlearning.CreateVisualizer();

					qlCNN.CNN.ValuesComputed += (data, isTraining) => { if(NetworkValuesUpdated != null) NetworkValuesUpdated(data, isTraining); };
                    break;
                }
            }
        }

        public static Action GetAction(QState state) {
            if(_instance == null) throw new InvalidOperationException("Manager in invalid state, call InitAgent first");
            switch (_instance.Mode) {
                case QAIMode.Learning:
                    if (_instance._sceneIsOver) return () => {};
                    return _instance._qlearning.GetLearningAction(state) ?? _instance.EndOfEpisode;
                case QAIMode.Testing:
                    return () => _instance.TesterAction(state);
                case QAIMode.Runnning:
                    return _instance._qlearning.GreedyPolicy(state).Action;
                default:
                    return () => {};
            }
        }

        private void EndOfEpisode() {
            if (_sceneIsOver) return;
            if(_qlearning.Iteration >= Terminator) {
                _qlearning.SaveModel();
                BenchmarkSave.WriteRunTime(_stopwatch.Elapsed.TotalSeconds);
                Debug.Log("Learning over after "+_stopwatch.Elapsed.TotalSeconds +" secounds");
                if (Benchmark) {
                    Debug.Log("Running Tester");
					ModeOverride = QAIMode.Testing;
                    _qlearning.LoadModel();
                    Application.LoadLevel(Application.loadedLevel);
                } else {
                    EditorApplication.isPlaying = false;
                    EditorApplication.Beep();
                }
            } else {
                Application.LoadLevel(Application.loadedLevel);
                _qlearning.Iteration++;
            }
            _sceneIsOver = true;
        }

        private void TesterAction(QState state) {
            if(Application.isLoadingLevel || _testIsOver) return;
            if(_testIsRunning) RunTest(state);
            else               SetupTest();
        }

        private void RunTest(QState state) {
            //End run if terminal
            if(state.IsTerminal) {
                Tester.OnTestComplete(state.Reward);
                _testIsRunning = false;
                Application.LoadLevel(Application.loadedLevel);
            //Take "best" action if test is running
            } else {
                var action = _qlearning.GreedyPolicy(state);
                action.Invoke();
                Tester.OnActionTaken(Agent, action, state);
            }
        }

        private void SetupTest() {
            var sceneSetup = Tester.SetupNextTest(Agent);
            //Run Test if tester have set up scene
            if(sceneSetup) {
                _testIsRunning = true;
            //End test run if tester says its over.
            } else {
                Tester.OnRunComplete();
                _testIsOver = true;
                if (Benchmark && BenchmarkSave.HaveRunsLeft) {
                    RemakeManager();
					ModeOverride = QAIMode.Learning;
                    Mode = QAIMode.Learning;
                    BenchmarkSave.NextRun();
                    Application.LoadLevel(Application.loadedLevel);
                } else {
                    EditorApplication.isPlaying = false;
                }
            }
        }

        private void RemakeManager() {
            _qlearning.Iteration = 1;
            _qlearning.RemakeModel(Agent.GetState());
            if (_visualizer != null) {
                Destroy(_visualizer.gameObject);
                _visualizer = _qlearning.CreateVisualizer();
            }
            _stopwatch.Reset();
            _stopwatch.Start();
            Tester.Init();
        }
        
        public static Dictionary<QAction, float> Query(QState state) {
            var q = _instance._qlearning.Q(state);
            return _instance._qlearning.Actions
                .ToDictionary(a => a, a => q(a));
        }

        public static void Imitate(QAgent agent, QState state, Action a) {
            if (_instance == null || _instance.Mode != QAIMode.Imitating) return;
            var terminal = _instance._imitation.Imitate(state, agent.ToQAction(a));
            if (terminal) {
//	        _instance._imitation.Save(); // Saving is now done in the Option Window, where the learning is started.
                EditorApplication.isPlaying = false;
            }
        }

        internal static void RunCoroutine(IEnumerator routine) {
            _instance.StartCoroutine(routine);
        }

        public static QImitationStorage SaveImitation(string name) {
            return _instance._imitation.CreateStorageItem(name);
        }

        void Awake() {
            if(_instance != null && _instance != this) Destroy(gameObject);
        }
    }

    public enum QAIMode {
        Runnning, Learning, Imitating, Testing
    }
}