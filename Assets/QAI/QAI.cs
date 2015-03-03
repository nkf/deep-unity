using System;
using System.Collections.Generic;
using Assets.QAI;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class QAI : MonoBehaviour {

    public const float TimeStep = 1f;
    public static void Run(QAgent agent) {
        _instance.StartCoroutine(RunAgent(agent.GetActions()));
    }
    
    //!!!!!!!!!!ITERATION AND TABLE CACHEING WILL NOT WORK WITH MULTIPLE AGENTS!!!!!!!!!!!
    public static int Iteration { get; private set; }

    public static void Restart(QTable table) {
        _table = table;
        Debug.Log("Restarting, I: " + Iteration);
        Application.LoadLevel(Application.loadedLevel);
    }

    private static QTable _table = null;

    public static void Learn(QAgent agent) {
        var qlearning = new QLearning(agent, _table);
        Iteration++;
        _instance.StartCoroutine(qlearning.Learn(Iteration));
    }

    private static IEnumerator RunAgent(IList<Action> actions) {
        while (true) {
            actions[Random.Range(0, actions.Count)].Invoke();
            yield return new WaitForSeconds(TimeStep);
        }
    }


    private static QAI _instance;
    void Awake() {
        var n = GameObject.FindGameObjectsWithTag("QAI").Length;
        if (n >= 1) {
            Destroy(gameObject);
        } else {
            DontDestroyOnLoad(gameObject);
            gameObject.tag = "QAI";
            _instance = this;
        }
    }
}
