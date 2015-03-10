using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class QAI : MonoBehaviour {
    public delegate void EpisodeCallback();

    public const float TIME_STEP = 1f;
    public const bool LEARNING = true; // TODO: User option.

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

    void Awake() {
        if (_instance == null) {
            _instance = this;
            if (LEARNING) {
                var woman = ActiveAgent.GetComponent<QAgent>();
                _qlearning = new QLearningNN(woman);
                _qlearning.RemakeModel();
                DontDestroyOnLoad(gameObject);
                StartCoroutine(_qlearning.RunEpisode(woman, EndOfEpisode));
            } // TODO: If not learning.
        } else {
            _instance.ActiveAgent = this.ActiveAgent;
            var woman = ActiveAgent.GetComponent<QAgent>(); // TODO: Multiple agents.
            _instance.StartCoroutine(_instance._qlearning.RunEpisode(woman, _instance.EndOfEpisode));
            Destroy(gameObject);
        }
    }
}
