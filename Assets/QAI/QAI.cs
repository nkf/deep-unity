using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Assets.QAI;
using UnityEngine;
using Random = UnityEngine.Random;

public class QAI : MonoBehaviour {

    public const float TIME_STEP = 1f;
    public const bool LEARNING = true; // TODO: User option.

    private static QAI _instance = null;
    private QLearning _qlearning;

    public void Learn(QAgent agent) {
        StartCoroutine(_qlearning.RunEpisode(agent));
        if (_qlearning.Iteration > 200) { // TODO: Termination condition.
            _qlearning.SaveModel();
        } else {
            Application.LoadLevel(Application.loadedLevel);
            Learn(GameObject.FindObjectOfType<GridWoman>());
        }
    }

    private static IEnumerator RunAgent(IList<QAction> actions) {
        while (true) {
            var legal = actions.Where(a => a.IsValid());
            actions[Random.Range(0, actions.Count)].Action.Invoke(); // TODO: Not random.
            yield return new WaitForSeconds(TIME_STEP);
        }
    }

    void Awake() {
        if (_instance == null) {
            _instance = this;
            var woman = GameObject.FindObjectOfType<GridWoman>(); // TODO: Specify agent.
            if (LEARNING) {
                _qlearning = new QLearning();
                DontDestroyOnLoad(gameObject);
                Learn(woman);
            } else {
                StartCoroutine(RunAgent(woman.GetQActions()));
            }
        } else {
            Destroy(gameObject);
        }
    }
}
