using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public abstract class QLearning {
    public const double TIE_BREAK = 1e-9;

    protected delegate double param(int i);
    protected virtual param Epsilon { get { return t => 0.2; } }
    protected virtual param StepSize { get { return t => 1.0 / t; } }
    protected virtual param Discount { get { return t => 1.0 / t; } }

    private readonly Random _rng;

    public QAgent Agent { get; private set; }
    public IList<QAction> Actions { get; private set; }
    public int Iteration { get; private set; }
	public bool Imitating { get; set; }

    public QLearning(QAgent agent) {
        _rng = new Random();
        Agent = agent;
        Actions = agent.GetQActions();
    }

    public IEnumerator<YieldInstruction> RunEpisode(QAgent agent, QAI.EpisodeCallback callback) {
        Iteration++;
        Agent = agent;
        Actions = agent.GetQActions();
        return Episode(callback);
    }

    protected virtual IEnumerator<YieldInstruction> Episode(QAI.EpisodeCallback callback) {
        var s = Agent.GetState();
        while (!s.IsTerminal) {
            var a = Imitating ? Agent.ConvertImitationAction() : Policy(s);
            var q = Q(s, a);
            a.Action.Invoke();
            var s0 = Agent.GetState();
            var a0max = Actions.Max(a0 => Q(s0, a0));
            var v = q + StepSize(Iteration) * (s0.Reward + Discount(Iteration) * a0max - q);
            Update(s, a, v);
            s = s0;
            yield return new WaitForEndOfFrame();
        }
        callback();
    }

    protected virtual QAction Policy(QState s) {
        if (Roll() < Epsilon(Iteration)) return Actions[Roll(Actions.Count)];
        return Actions.Where(a => a.IsValid()).OrderByDescending(a => Q(s, a) + Roll() * TIE_BREAK).First();
    }

    protected abstract double Q(QState s, QAction a);
    protected abstract void Update(QState s, QAction a, double v);

    public abstract void SaveModel();
    public abstract void LoadModel();
    public abstract void RemakeModel();
    public abstract QAction BestAction();

    protected int Roll(int bound) {
        return _rng.Next(bound);
    }

    protected double Roll() {
        return _rng.NextDouble();
    }
}
