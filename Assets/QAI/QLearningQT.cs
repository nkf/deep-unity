using System.Collections;
using System.Linq;
using UnityEngine;
using Random = System.Random;

class QLearningQT : QLearning {
    private readonly QTable _QTable;
    private readonly QAgent _agent;
    private readonly Random _rng;
    private delegate double Param(int i);

    private const string QTableName = "QTable.xml";

    public QLearningQT(QAgent agent, QTable table = null) {
        _agent = agent;
        _rng = new Random();
        
        if (table == null) {
            _QTable = new QTable(defaultReward: 0);
            _QTable.Load(QTableName);
        } else {
            _QTable = table;
        }
    }

    private double Q(QState s, QAction a) {
        return _QTable.Query(s, a);
    }

    private readonly Param _eps = t => 0.8;
    private QAction EpsilonGreedy(QState s, QAction[] actions, int t) {
        if(_rng.NextDouble() < _eps(t)) return actions[_rng.Next(actions.Length)];
        var ordered = actions.Select(a => new { A = a, Q = Q(s, a) }).OrderByDescending(x => x.Q);
        var highest = ordered.First().Q;
        var highArray = ordered.Where(x => x.Q == highest).ToArray();
        return highArray[_rng.Next(highArray.Length)].A;
    }

    private readonly Param _discount = t => 1.0 / t;
    private readonly Param _stepsize = t => 1.0 / t;
    public IEnumerator Learn(int iteration) {
        var t = 1;
        var s = _agent.GetState();
        var actions = _agent.GetQActions();
        while (!s.IsTerminal) {
            var a = EpsilonGreedy(s, actions, t);
            a.Action.Invoke();
            var s0 = _agent.GetState();
            var a0max = actions.Max(a0 => Q(s0, a0));
            var v = Q(s, a) + _stepsize(t) * (s0.Reward + _discount(t) * a0max - Q(s, a));
            _QTable.Add(s,a,v);
            s = s0;
            t++;
            yield return new WaitForEndOfFrame();
        }
        if (iteration%10 == 0)
            _QTable.Save(QTableName);
        QAI.Restart(_QTable);
    }
   
}
