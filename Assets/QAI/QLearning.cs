using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Assets.QAI;
using UnityEngine;
using Random = System.Random;

class QLearning {
    private readonly QTable _QTable;
    private readonly QAgent _agent;
    private readonly Random _rng;

    private const string QTablePath = "JOHN.xml";

    public QLearning(QAgent agent) {
        _agent = agent;
        _rng = new Random();
        _QTable = new QTable();
        _QTable.Load(QTablePath);
    }

    private double Q(QState s, QAction a) {
        return _QTable.Query(s, a);
    }

    private const double Epsilon = 1;
    private QAction EpsilonGreedy(QState s, QAction[] actions) {
        if (_rng.NextDouble() > Epsilon) return actions[_rng.Next(actions.Length)];
        var ordered = actions.Select(a => new {A = a, Q = Q(s, a)}).OrderByDescending(x => x.Q);
        var highest = ordered.First().Q;
        var highArray = ordered.Where(x => x.Q == highest).ToArray();
        return highArray[_rng.Next(highArray.Length)].A;
    }

    private const double Discount = 0.5;
    public IEnumerator Learn() {
        var t = 1;
        var s = _agent.GetState();
        while (!s.IsTerminal) {
            var a = EpsilonGreedy(s, _agent.GetQActions());
            a.Action.Invoke();
            var s0 = _agent.GetState();
            var a0max = _agent.GetQActions().Max(a0 => Q(s0, a0));
            var v = Q(s, a) + 1.0/t * (s0.Reward + Discount * a0max - Q(s, a));
            _QTable.Add(s,a,v);
            s = s0;
            t++;
            yield return new WaitForEndOfFrame();
        }
        _QTable.Save(QTablePath);
        Application.LoadLevel(0);
    }


}
