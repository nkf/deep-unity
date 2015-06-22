using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class QAI : MonoBehaviour {
    public delegate void EpisodeCallback();

    public const float TimeStep = 0.3f;
    [HideInInspector] 
    public QAIMode Mode;
    [HideInInspector]
    public bool Remake;
    [HideInInspector]
    public int Terminator;

    [HideInInspector]
    public List<QStory> Stories;
	
	public GameObject ActiveAgent;
	public int Iteration { get { return _qlearning == null ? 0 : _qlearning.Iteration; }}

    public QTester Tester;

    private static QAI _instance = null;
    private QLearningCNN _qlearning;
    private QImitation _imitation;

    private QAgent _agent;
    private bool _testIsRunning;

    public static int NumIterations() {
        return _instance == null ? 1 : _instance.Terminator;
    }

    public static void InitAgent(QAgent agent) {
        if (_instance == null) {
            _instance = FindObjectOfType<QAI>();
            _instance.Init(agent);
        }
        _instance._agent = agent;
        if(_instance.Mode != QAIMode.Imitating) 
			_instance._qlearning.Agent = agent;
    }

    private void Init(QAgent agent) {
        DontDestroyOnLoad(gameObject);
        switch (Mode) {
            case QAIMode.Imitating: {
                _imitation = new QImitation();
                break;
            }
            default: {
                _qlearning = new QLearningCNN {Agent = agent};
                if(Remake) _qlearning.RemakeModel();
                else _qlearning.LoadModel();
                break;
            }
        }
    }

    public static Action GetAction(QState state) {
        switch (_instance.Mode) {
            case QAIMode.Learning:
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
        if(_qlearning.Iteration >= Terminator) {
            _qlearning.SaveModel();
            EditorApplication.isPlaying = false;
        } else {
            Application.LoadLevel(Application.loadedLevel);
        }
        _qlearning.Iteration++;
    }

    private void TesterAction(QState state) {
        if(_testIsRunning) RunTest(state);
        else               SetupTest(state);
    }
    private void RunTest(QState state) {
        Time.timeScale = TimeStep * 6;
        //End run if terminal
        if(state.IsTerminal) {
            Tester.OnTestComplete(state.Reward);
            _testIsRunning = false;
            Application.LoadLevel(Application.loadedLevel);
        //Take "best" action if test is running
        } else {
            var sars = _agent.MakeSARS(_qlearning.GreedyPolicy(state));
            Tester.OnActionTaken(_agent, sars);
        }
    }

    private void SetupTest(QState state) {
        var sceneSetup = Tester.SetupNextTest(_agent);
        //End test run if tester says its over.
        if(!sceneSetup) {
            Tester.OnRunComplete();
            EditorApplication.isPlaying = false;
        //Run Test if tester have set up scene
        } else {
            _testIsRunning = true;
            RunTest(state);
        }
    }

	public static void Imitate(QAgent agent, Action a) {
	    if (_instance == null || _instance.Mode != QAIMode.Imitating) return;
        var terminal = _instance._imitation.Imitate(agent, agent.ToQAction(a));
	    if (terminal) {
//	        _instance._imitation.Save(); // Saving is now done in the Option Window, where the learning is started.
	        EditorApplication.isPlaying = false;
	    }
	}

    internal static QImitationStorage SaveImitation(string name) {
        return _instance._imitation.CreateStorageItem(name);
    }

    void Awake() {
        if(_instance != null && _instance != this) Destroy(gameObject);
    }
}

public enum QAIMode {
    Runnning, Learning, Imitating, Testing
}
