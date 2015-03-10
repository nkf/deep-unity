using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;

public class QAI : MonoBehaviour {
    public delegate void EpisodeCallback();

    public const float TIME_STEP = 1f;
	[HideInInspector]
    public bool LEARNING = true; // TODO: User option.
	[HideInInspector]
	public bool IMITATING;

    public GameObject ActiveAgent;

    private static QAI _instance = null;
    private QLearning _qlearning;
    
    public void EndOfEpisode() {
        if (_qlearning.Iteration > 200) { // TODO: Termination condition.
            _qlearning.SaveModel();
        } else {
            Application.LoadLevel(Application.loadedLevel);
        }
    }

    private IEnumerator<YieldInstruction> RunAgent(IList<QAction> actions) {
        _qlearning.LoadModel();
        while (true) {
            var legal = actions.Where(a => a.IsValid());
            legal.ElementAt(Random.Range(0, actions.Count)).Action.Invoke(); // TODO: Not random.
            yield return new WaitForSeconds(TIME_STEP);
        }
    }

	private IEnumerator _imitationProcess;
	public static void Imitate(QAgent agent) {
		if(!_instance.IMITATING) return;
		if(_instance._imitationProcess == null) 
			_instance._imitationProcess = _instance._qlearning.RunEpisode(agent, _instance.EndOfEpisode);
		if(!_instance._imitationProcess.MoveNext())
			_instance._imitationProcess = null;
	}

    void Awake() {
        if (_instance == null) {
            _instance = this;
            if (LEARNING) {
                var woman = ActiveAgent.GetComponent<QAgent>();
                _qlearning = new QLearningQT(woman) {Imitating = IMITATING};
                _qlearning.RemakeModel();
                DontDestroyOnLoad(gameObject);
				if(!IMITATING)
                	StartCoroutine(_qlearning.RunEpisode(woman, EndOfEpisode));
            } // TODO: If not learning.
        } else {
            _instance.ActiveAgent = this.ActiveAgent;
            var woman = ActiveAgent.GetComponent<QAgent>(); // TODO: Multiple agents.
			if(!IMITATING)
            	_instance.StartCoroutine(_instance._qlearning.RunEpisode(woman, _instance.EndOfEpisode));
            Destroy(gameObject);
        }
    }
}
