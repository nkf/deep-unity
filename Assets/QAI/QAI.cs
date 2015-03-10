﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;

public class QAI : MonoBehaviour {
    public delegate void EpisodeCallback();

    public const float TIME_STEP = 1f;
    [HideInInspector]
    public bool LEARNING;
	[HideInInspector]
	public bool IMITATING;
    [HideInInspector]
    public int TERMINATOR;

    public GameObject ActiveAgent;

    private static QAI _instance = null;
    private QLearning _qlearning;
    
    public void EndOfEpisode() {
        if (_qlearning.Iteration > TERMINATOR) {
            _qlearning.SaveModel();
        } else {
            Application.LoadLevel(Application.loadedLevel);
        }
    }

    private IEnumerator<YieldInstruction> RunAgent() {
        while (true) {
            _qlearning.BestAction().Action.Invoke();
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
            var woman = ActiveAgent.GetComponent<QAgent>();
            //_qlearning = new QLearningQT(woman) { Imitating = IMITATING };
            _qlearning = new QLearningNN(woman);
            if (LEARNING) {
                _qlearning.RemakeModel();
                DontDestroyOnLoad(gameObject);
                if (!IMITATING)
                    StartCoroutine(_qlearning.RunEpisode(woman, EndOfEpisode));
            } else {
                _qlearning.LoadModel();
                StartCoroutine(RunAgent());
            }
        } else {
            _instance.ActiveAgent = this.ActiveAgent;
            var woman = ActiveAgent.GetComponent<QAgent>(); // TODO: Multiple agents.
			if(!IMITATING)
            	_instance.StartCoroutine(_instance._qlearning.RunEpisode(woman, _instance.EndOfEpisode));
            Destroy(gameObject);
        }
    }
}