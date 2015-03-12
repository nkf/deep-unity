using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class QLearningNN {
    private delegate double ActionValueFunction(QAction a);

    public const double TIE_BREAK = 1e-9;
    public const string MODEL_PATH = "JOHN_N.csv";

    private QLearning.param Epsilon = t => 25 - t / 4;
    private QLearning.param Discount = t => 0.99;
    private QLearning.param StepSize = t => 0.5;

    public QAgent Agent { get; private set; }
    public IList<QAction> Actions { get; private set; }
    public int Iteration { get; private set; }

    private QNetwork _net;
    private Dictionary<string, int> _amap;
    private double[] _output;
    private readonly bool _imit;
    private readonly Random _rng;

    public QLearningNN(QAgent agent, bool imitating = false) {
        _rng = new Random();
        _imit = imitating;
        Agent = agent;
        Actions = agent.GetQActions();
    }

    public void LoadModel() {
        _net = QNetwork.Load(MODEL_PATH);
        _amap = new Dictionary<string, int>();
        int ix = 0;
        foreach (QAction a in Actions)
            _amap[a.ActionId] = ix++;
    }

    public void SaveModel() {
        _net.Save(MODEL_PATH);
    }

    public void RemakeModel() {
        int size = Agent.GetState().Features.Length;
        _net = new QNetwork(size, Actions.Count, 1, size * 2);
        _amap = new Dictionary<string, int>();
        int ix = 0;
        foreach (QAction a in Actions)
            _amap[a.ActionId] = ix++;
    }

    public IEnumerator<YieldInstruction> RunEpisode(QAgent agent, QAI.EpisodeCallback callback) {
        Iteration++;
        Agent = agent;
        Actions = agent.GetQActions();
        var s = Agent.GetState();
        while (!s.IsTerminal) {
            var a = _imit ? Agent.ConvertImitationAction() : Policy(s);
            a.Action.Invoke();
            var s0 = Agent.GetState();
            if (!s0.IsTerminal) {
                var q0 = Q(s0);
                var a0max = Actions.Max(a0 => q0(a0));
                var target = s0.Reward + Discount(Iteration) * a0max;
                Q(s);
                Update(s, a, target);
                s = s0;
            } else {
                Update(s, a, s0.Reward);
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        callback();
    }

    private ActionValueFunction Q(QState s) {
        _net.Feedforward(s.Features.Select(f => (double)f).ToArray());
        _output = _net.Output().ToArray();
        return a => _output[_amap[a.ActionId]];
    }

    private void Update(QState s, QAction a, double v) {
        _output[_amap[a.ActionId]] = v;
        _net.Backpropagate(_output, StepSize(Iteration));
    }

    private QAction Policy(QState s) {
        var q = Q(s);
        if (_rng.NextDouble() < Epsilon(Iteration)) return Actions[_rng.Next(Actions.Count)];
        return Actions.Where(a => a.IsValid()).OrderByDescending(a => q(a) + _rng.NextDouble() * TIE_BREAK).First();
    }

    public QAction BestAction() {
        var s = Agent.GetState();
        var q = Q(s);
        return Actions.Where(a => a.IsValid()).OrderByDescending(a => q(a)).First();
    }
}
