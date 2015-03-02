using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class QAI : MonoBehaviour {

    public const float TimeStep = 1f;
    public static void Run(QAgent agent) {
        _instance.StartCoroutine(RunAgent(agent.GetActions()));
    }

    public static void Learn(QAgent agent) {
        var qlearning = new QLearning(agent);
        _instance.StartCoroutine(qlearning.Learn());
    }

    private static IEnumerator RunAgent(IList<Action> actions) {
        while (true) {
            actions[Random.Range(0, actions.Count)].Invoke();
            yield return new WaitForSeconds(TimeStep);
        }
    }


    private static QAI _instance;
    void Awake() {
        _instance = this;
    }
}
