using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class QApx : QMethod {
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
    private readonly Random _rng = new Random();
    private readonly Func<int, double> _eps = t => 0.0;
    public QAction Policy(QState s, IEnumerable<QAction> actions, int t) {
        if(_rng.NextDouble() < _eps(t)) {
            var qActions = actions.ToArray();
            return qActions[_rng.Next(qActions.Length)];
        }
        return actions
            .Select(a => new {q = Q(s, a), a })
            .MaxWithRandomTie(x => x.q).a;
    }
    private readonly Func<int, double> _discount = t => 0.5;
    private readonly Func<int, double> _stepsize = t => 1.0 / t;
    public void Update(QAction a, double r, QState s, IEnumerable<QAction> ap, QState sn, int t) {
        var nMax = ap.Max(an => Q(sn, an));
        var loss = _stepsize(t)*(r + _discount(t)*nMax - Q(s, a));
        for (int k = 0; k < _weights[a].Length; k++) {
            _weights[a][k] += loss * s.Features[k];
        }
    }

    public void Save(string path) {
        QData.Save(path, _weights);
    }

    public void Load(string path) {
        QData.Load(path, _weights);
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
