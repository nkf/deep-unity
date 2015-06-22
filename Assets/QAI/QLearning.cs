using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public abstract class QLearning {

    private const double TIE_BREAK = 1e-9;
    private readonly Random rng = new Random();

    private QAgent _agent;
    public QAgent Agent { get { return _agent; } set { SetAgent(value);} }
    public ReadOnlyCollection<QAction> Actions { get; private set; }
    public int Iteration { get; set; }

    public delegate double Param(double t);
    public delegate double ActionValueFunction(QAction a);

    public abstract void SaveModel();
    public abstract void LoadModel();
    public abstract void RemakeModel();

    public abstract ActionValueFunction Q(QState s);
    public abstract IEnumerator<YieldInstruction> RunEpisode(QAI.EpisodeCallback callback);

    private void SetAgent(QAgent agent) {
        _agent = agent;
        Actions = agent.GetQActions().AsReadOnly();
    }

    public QAction GreedyPolicy() {
        return GreedyPolicy(Agent.GetState());
    }

    public QAction GreedyPolicy(QState s) {
        var q = Q(s);
        return ValidActions().Select(a => new { v = q(a) + TIE_BREAK * rng.NextDouble(), a }).OrderByDescending(va => va.v).First().a;
    }

    public QAction ProbabilisticPolicy() {
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
        return EpsilonPolicy(eps, ProbabilisticPolicy);
    }

    public QAction EpsilonPolicy(double eps, Func<QAction> policy) {
        if(rng.NextDouble() < eps) {
            var valid = ValidActions();
            return valid[rng.Next(valid.Count)];
        }
        return policy();
    }

    /// <summary>
    /// Always returns a non-empty list. Contains the NullAction if no valid actions are available.
    /// </summary>
    /// <returns>List of valid actions.</returns>
    protected IList<QAction> ValidActions() {
        var actions = Actions.Where(a => a.IsValid()).ToList();
        if (actions.Count == 0)
            actions.Add(QAction.NullAction);
        return actions;
    }
}
