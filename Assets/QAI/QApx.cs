using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Assets.QAI;
using UnityEngine;
using System.Collections;

public class QApx {
    private readonly SerializableDictionary<QAction, double[]> _weights;

    public QApx(IEnumerable<QAction> actions, int numFeatures) {
        _weights = new SerializableDictionary<QAction, double[]>();
        foreach (var a in actions) {
            _weights[a] = new double[numFeatures];
        }
    }


    public double Q(QState s, QAction a) {
        return s.Features.Select((t, i) => _weights[a][i]*t).Sum();
    }

    public QAction Policy(QState s) {
        return _weights.Keys
            .Select(a => new {Q = Q(s,a), A = a})
            .MaxWithRandomTie(x => x.Q).A;
    }

    public void Update(QAction a, double r, QState s, QAction[] ap, QState sn) {
        for (int k = 0; k < _weights[a].Length; k++) {
            var theta = _weights[a][k];
            var nMax = ap.Max(an => Q(sn, an));
            _weights[a][k] += /*ALPHA?*/ (r* /*GAMMA * ?*/nMax - Q(s, a))*(Q(s, a)/theta);
        }
    }


    private QApx() {
        _weights = new SerializableDictionary<QAction, double[]>();
    }
    public QApx Copy() {
        var copy = new QApx();
        foreach (var entry in _weights) {
            copy._weights[entry.Key] = (double[])entry.Value.Clone();
        }
        return copy;
    }
}
