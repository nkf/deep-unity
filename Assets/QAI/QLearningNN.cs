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
    private bool _updated = true;
    private Dictionary<string, int> _amap;
    private double[] _output;

    public QLearningNN(QAgent agent) : base(agent) { }

    public override void LoadModel() {
        throw new NotImplementedException();
    }

    public override void SaveModel() {
        // Woops.
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
        if (_updated) {
            _net.Feedforward(s.Features.Select(f => (double)f).ToArray());
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
}
