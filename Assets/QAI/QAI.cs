using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class QAI : MonoBehaviour {
    public delegate void EpisodeCallback();

    public const float TimeStep = 1f;
    [HideInInspector]
    public bool Learning;
	[HideInInspector]
	public bool Imitating;
    [HideInInspector]
    public int Terminator;
    [HideInInspector] 
    public List<QStory> Stories; 


    public GameObject ActiveAgent;

    private static QAI _instance = null;
    private QLearning _qlearning;


    public void EndOfEpisode() {
        if (_qlearning.Iteration > Terminator) {
            _qlearning.SaveModel();
            Application.Quit();
        } else {
            Application.LoadLevel(Application.loadedLevel);
        }
    }

    private IEnumerator<YieldInstruction> RunAgent() {
        while (true) {
            _qlearning.BestAction().Invoke();
            yield return new WaitForSeconds(TimeStep);
        }
    }

	private IEnumerator _imitationProcess;
	public static void Imitate(QAgent agent) {
		if(!_instance.Imitating) return;
		if(_instance._imitationProcess == null) 
			_instance._imitationProcess = _instance._qlearning.RunEpisode(agent, _instance.EndOfEpisode);
		if(!_instance._imitationProcess.MoveNext())
			_instance._imitationProcess = null;
	}

    void Awake() {
        if (_instance == null) {
            _instance = this;
            var woman = ActiveAgent.GetComponent<QAgent>();
            //_qlearning = new QLearningQT(woman) { Imitating = IMITATING };
            _qlearning = new QLearningNN(woman);
            if (Learning) {
                _qlearning.RemakeModel();
                DontDestroyOnLoad(gameObject);
                if (!Imitating)
                    StartCoroutine(_qlearning.RunEpisode(woman, EndOfEpisode));
            } else {
                _qlearning.LoadModel();
                StartCoroutine(RunAgent());
            }
        } else {
            _instance.ActiveAgent = this.ActiveAgent;
            var woman = ActiveAgent.GetComponent<QAgent>(); // TODO: Multiple agents.
			if(!Imitating)
            	_instance.StartCoroutine(_instance._qlearning.RunEpisode(woman, _instance.EndOfEpisode));
            Destroy(gameObject);
        }
    }
}
