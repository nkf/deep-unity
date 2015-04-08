using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public abstract class QLearning {

    private const double TIE_BREAK = 1e-9;
    private Random rng = new Random();

    protected QAgent Agent { get; private set; }
    protected IList<QAction> Actions { get; private set; }
    public int Iteration { get; protected set; }

    public delegate double Param(double t);
    public delegate double ActionValueFunction(QAction a);

    public abstract void SaveModel();
    public abstract void LoadModel();
    public abstract void RemakeModel();

    public abstract ActionValueFunction Q(QState s);
    public abstract IEnumerator<YieldInstruction> RunEpisode(QAI.EpisodeCallback callback);

    public void SetAgent(QAgent agent) {
        Agent = agent;
        Actions = agent.GetQActions();
    }

    public QAction GreedyPolicy() {
        var s = Agent.GetState();
        var q = Q(s);
        return ValidActions().Select(a => new { v = q(a) + TIE_BREAK * rng.NextDouble(), a }).OrderByDescending(va => va.v).First().a;
    }

    public QAction PropabalisticPolicy() {
        var s = Agent.GetState();
        var q = Q(s);
        var vas = ValidActions().Select(a => new {v = (int)(q(a) * 100), a}).ToList();
        var roll = rng.Next(vas.Sum(va => va.v));
        var n = 0;
        foreach (var va in vas) {
            if (roll >= n && roll < n + va.v) return va.a;
            n += va.v;
        }
        throw new Exception("falsted failed at math.");
    }

    public QAction EpsilonGreedy(double eps) {
        return EpsilonPolicy(eps, GreedyPolicy);
    }

    public QAction EpsilonPropabalistic(double eps) {
        return EpsilonPolicy(eps, PropabalisticPolicy);
    }

    public QAction EpsilonPolicy(double eps, Func<QAction> policy) {
        if(rng.NextDouble() < eps) {
            var valid = ValidActions();
            return valid[rng.Next(valid.Count)];
        }
        return policy();
    }

    protected IList<QAction> ValidActions() {
        return Actions.Where(a => a.IsValid()).ToList();
    }
}
