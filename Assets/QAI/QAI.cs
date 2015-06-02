using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class QAI : MonoBehaviour {
    public delegate void EpisodeCallback();

    public const float TimeStep = 0.3f;
    [HideInInspector]
    public bool Learning;
    [HideInInspector]
    public bool Remake;
	[HideInInspector]
	public bool Imitating;
    [HideInInspector]
    public int Terminator;
	[HideInInspector]
	public bool Testing;
    [HideInInspector]
    public List<QStory> Stories; 
	
	public GameObject ActiveAgent;
	public int Iteration { get { return _qlearning == null ? 0 : _qlearning.Iteration; }}

    public QTester Tester;
    private bool _abortTestRun = false;

    private static QAI _instance = null;
    private QLearningDBN _qlearning;
    private QImitation _imitation;

    public static int NumIterations() {
        return _instance == null ? 1 : _instance.Terminator;
    }

    private void EndOfEpisode() {
        if (_qlearning.Iteration >= Terminator) {
            _qlearning.SaveModel();
            EditorApplication.isPlaying = false;
        } else {
            Application.LoadLevel(Application.loadedLevel);
        }
    }

    private IEnumerator<YieldInstruction> RunAgent() {
        while (true) {
            var a = _qlearning.GreedyPolicy();
            Debug.Log(a);
            a.Invoke();
            //yield return new WaitForSeconds(TimeStep);
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator<YieldInstruction> RunTester(QAgent agent) {
        _abortTestRun = false;
        while(!agent.GetState().IsTerminal) {
            //var a = _qlearning.GreedyPolicy();
            var a = _qlearning.EpsilonGreedy(0.1f);
            //var a = _qlearning.PropabalisticPolicy();
            Tester.OnActionTaken(agent, agent.MakeSARS(a));
            //yield return new WaitForEndOfFrame();
            yield return new WaitForFixedUpdate();
            if (_abortTestRun) break;
        }
        Tester.OnTestComplete(agent.GetState().Reward);
        Application.LoadLevel(Application.loadedLevel);
    }

    public static void EndTestRun() {
        _instance._abortTestRun = true;
    }

	public static void Imitate(QAgent agent, Action a) {
	    if (_instance == null || !_instance.Imitating) return;
        var terminal = _instance._imitation.Imitate(agent, agent.ToQAction(a));
	    if (terminal) {
//	        _instance._imitation.Save(); // Saving is now done in the Option Window, where the learning is started.
	        EditorApplication.isPlaying = false;
	    }
	}

    internal static QImitationStorage SaveImitation(string name) {
        return _instance._imitation.CreateStorageItem(name);
    }


    void Start() {
        if (_instance == null) {
            _instance = this;
            var agent = ActiveAgent.GetComponent<QAgent>();
            if (Imitating) {
                _imitation = new QImitation();
            } else {
                _qlearning = new QLearningDBN();
                _qlearning.SetAgent(agent);
                DontDestroyOnLoad(gameObject);
                if (Learning) {
                    Time.timeScale = 2;
                    if (Remake)
                        _qlearning.RemakeModel();
                    else
                        _qlearning.LoadModel();
                    StartCoroutine(_qlearning.RunEpisode(EndOfEpisode));
                } else if (Testing) {
                    _qlearning.LoadModel();
                    var sceneSetup = Tester.SetupNextTest(agent);
                    if (sceneSetup) StartCoroutine(RunTester(agent));
                    else {
                        _instance.Tester.OnRunComplete();
                        EditorApplication.isPlaying = false;
                    }
                } else {
                    _qlearning.LoadModel();
                    StartCoroutine(RunAgent());
                }
            }
        } else {
            _instance.ActiveAgent = ActiveAgent;
            var agent = ActiveAgent.GetComponent<QAgent>(); // TODO: Multiple agents.
            _instance._qlearning.SetAgent(agent);
            if (!_instance.Imitating && _instance.Learning) {
                _instance.StartCoroutine(_instance._qlearning.RunEpisode(_instance.EndOfEpisode));
            } else if (_instance.Testing) {
                var sceneSetup = _instance.Tester.SetupNextTest(agent);
                if (sceneSetup) _instance.StartCoroutine(_instance.RunTester(agent));
                else {
                    _instance.Tester.OnRunComplete();
                    EditorApplication.isPlaying = false;
                }
            }
            Destroy(gameObject);
        }
    }
}
