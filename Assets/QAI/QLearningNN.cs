using System;
using System.Collections.Generic;
using System.Linq;

public class QLearningNN : QLearning {

    private const string MODEL_PATH = "JOHN_N.csv";

    protected override QLearning.param Epsilon {
        get {
            return t => 50 - t;
        }
    }

    protected override QLearning.param Discount {
        get {
            return t => 0.9;
        }
    }

    private QNetwork _net;
    private Dictionary<string, int> _amap;
    private int[] _cache;
    private double[] _output;
    private bool _updated = false;

    public QLearningNN(QAgent agent) : base(agent) { }

    public override void LoadModel() {
        _net = QNetwork.Load(MODEL_PATH);
        _amap = new Dictionary<string, int>();
        int ix = 0;
        foreach (QAction a in Actions)
            _amap[a.ActionId] = ix++;
    }

    public override void SaveModel() {
        _net.Save(MODEL_PATH);
    }

    public override void RemakeModel() {
        int size = Agent.GetState().Features.Length;
        _net = new QNetwork(size, Actions.Count, 1, size * 2);
        _amap = new Dictionary<string, int>();
        int ix = 0;
        foreach (QAction a in Actions)
            _amap[a.ActionId] = ix++;
    }

    protected override double Q(QState s, QAction a) {
        if (s.Features != _cache || _updated) {
            _net.Feedforward(s.Features.Select(f => (double)f).ToArray());
            _cache = s.Features;
            _output = _net.Output().ToArray();
            _updated = false;
        }
        return _output[_amap[a.ActionId]];
    }

    protected override void Update(QState s, QAction a, double v) {
        _output[_amap[a.ActionId]] = v;
        _net.Backpropagate(_output);
        _updated = true;
    }

    public override QAction BestAction() {
        var s = Agent.GetState();
        return Actions.Where(a => a.IsValid()).OrderByDescending(a => Q(s, a)).First();
    }
}
