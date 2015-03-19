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
        return ValidActions().OrderByDescending(a => q(a) + TIE_BREAK).First();
    }

    public QAction EpsilonGreedy(double eps) {
        if (rng.NextDouble() < eps) {
            var valid = ValidActions();
            return valid[rng.Next(valid.Count)];
        }
        return GreedyPolicy();
    }

    protected IList<QAction> ValidActions() {
        return Actions.Where(a => a.IsValid()).ToList();
    }
}
