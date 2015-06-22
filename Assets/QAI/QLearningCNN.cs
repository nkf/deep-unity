using System.Collections.Generic;
using System.Linq;
using C5;
using MathNet.Numerics.LinearAlgebra;
using QNetwork.CNN;
using QNetwork.Training;
using UnityEditor;
using UnityEngine;

public class QLearningCNN : QLearning {
    public const string MODEL_PATH = "QData/JOHN_N";

    private const float EpisilonStart = 0.5f;
    private const float EpisilonEnd = 0.1f;
    private readonly Param Epsilon = t => EpisilonStart - ((EpisilonEnd - EpisilonStart) / QAI.NumIterations()) * t;
    private const float Discount = 0.99f;

    private const bool PrioritySweeping = false;

    private const int BatchSize = 10;
    private const int PredecessorCap = 6;
    private const float PriorityThreshold = 0.01f;

    private const float LearningRate = 0.1f;
    private const float Momentum = 0.9f;

    private int _size;
    private ConvolutionalNetwork _net;
    private Backprop<Matrix<float>[]> _trainer;
    private List<SARS> _imitationExps;
    private QExperience _qexp;
    private Dictionary<string, int> _amap;
    private Vector<float> _output;

    private Dictionary<QState, List<SARS>> _preds = new Dictionary<QState,List<SARS>>(1000);
    private IntervalHeap<SARS> _pq = new IntervalHeap<SARS>(200, new SARSPrioritizer());

    private class SARSPrioritizer : IComparer<SARS> {
        public int Compare(SARS x, SARS y) {
            return (int)(x.Priority - y.Priority);
        }
    }

    private void Initialize() {
        LoadExperienceDatabase();

        // Action-index mapping.
        _amap = new Dictionary<string, int>();
        int ix = 0;
        foreach(QAction a in Actions)
            _amap[a.ActionId] = ix++;
    }

    public override void LoadModel() {
        Initialize();
        _net = ConvolutionalNetwork.Load(MODEL_PATH);
        //_net = MultiLayerPerceptron.Load(MODEL_PATH);
        _trainer = new Backprop<Matrix<float>[]>(_net, LearningRate, Momentum);
    }

    public override void SaveModel() {
        _net.Save(MODEL_PATH);
    }

    public override void RemakeModel() {
        Initialize();
        _size = Agent.GetState().Features[0].RowCount;
        _net = new ConvolutionalNetwork(_size, _amap.Count, new CNNArgs {FilterSize = 5, FilterCount = 3, PoolLayerSize = 2, Stride = 2});
        //_net = new MultiLayerPerceptron(5, new[] { 50, 3 });
        _trainer = new Backprop<Matrix<float>[]>(_net, LearningRate, Momentum);
    }

    public override IEnumerator<YieldInstruction> RunEpisode(QAI.EpisodeCallback callback) {
        Iteration++;
        var s = Agent.GetState();
        var a = default(QAction);
        var prevS = default(QState);
        while(!s.IsTerminal) {
            if (s.Equals(prevS)) {
                a.Invoke();
                s = Agent.GetState();
                yield return new WaitForFixedUpdate();
                continue;
            }
            // Experience step.
            a = EpsilonGreedy(Epsilon(Iteration));
            var sars = Agent.MakeSARS(a);
            if (PrioritySweeping) {
                PutPredecessor(sars);
                EnqueueSARS(sars);
                while (_pq.Count > 100)
                    _pq.DeleteMin();
            } else {
                _qexp.Store(sars, 100);
            }
            s = sars.NextState;
            prevS = sars.State;
            // Learning step.
            if (PrioritySweeping)
                PrioritizedSweeping();
            else
                TrainModel();

            // End of frame.
            yield return new WaitForFixedUpdate();
        }
        callback();
    }
    
    public override ActionValueFunction Q(QState s) {
        _output = _net.Compute(s.Features);
        //Debug.Log(string.Join(";", _output.Select(v => string.Format("{0:.00}", v)).ToArray()) + " ~ " + string.Format("{0:.000}", _output.Average()));
        return a => _output[_amap[a.ActionId]];
    }

    public void LoadExperienceDatabase() {
        _imitationExps = QStory.LoadAll("QData/Story")
            .Where(qs => qs.ScenePath == EditorApplication.currentScene)
            .SelectMany(qs => qs.ImitationExperiences.Select(qi => qi.Experience))
            .SelectMany(e => e).ToList();
        Debug.Log("Loading " + _imitationExps.Count + " imitation experiences");
        _qexp = new QExperience();
    }

    public List<SARS> SampleBatch(int size) {
        //var r = _imitationExps.Random().Concat(_qexp.Random()).ToList();
        //var r = _imitationExps.Concat(_qexp).Shuffle().Take(size).ToList();
        var r = _qexp.Shuffle().Take(size).ToList();
        //var r = _imitationExps.Shuffle().ToList();
        return r;
    }

    private void TrainModel() {
        var batch = SampleBatch(BatchSize);
        var inp = new Matrix<float>[batch.Count][];
        var outp = new TargetIndexPair[batch.Count];
        int i = 0;
        foreach (var sars in batch) {
            inp[i] = sars.State.Features;
            float target;
            if (!sars.NextState.IsTerminal) {
                var a0max = _net.Compute(sars.NextState.Features).Max();
                target = sars.Reward + Discount * a0max;
            } else {
                target = sars.Reward;
            }
            /*
            // ATTENTION: Not Q-learning.
            // Delete from here.
            var ideal = Vector<float>.Build.Dense(3);
            for (int n = 0; n < ideal.Count; n++)
                ideal[n] = 0f;
            var target = 1f;
            // To here.
            */
            outp[i++] = new TargetIndexPair(target, _amap[sars.Action.ActionId]);
        }
        for (int j = 0; j < batch.Count; j++) {
            _trainer.SGD(inp[j], outp[j]);
        }
    }

    private void PrioritizedSweeping() {
        int N = Mathf.Min(BatchSize, _pq.Count);
        var inp = new Matrix<float>[N][];
        var outp = new TargetIndexPair[N];
        for (int i = 0; i < N; i++) {
            var sars = _pq.DeleteMax();
            inp[i] = sars.State.Features;
            float target;
            if (!sars.NextState.IsTerminal) {
                var a0max = _net.Compute(sars.NextState.Features).Max();
                target = sars.Reward + Discount * a0max;
            } else {
                target = sars.Reward;
            }
            outp[i] = new TargetIndexPair(target, _amap[sars.Action.ActionId]);
            if (_preds.ContainsKey(sars.State))
                foreach (var pred in _preds[sars.State].Shuffle().Take(PredecessorCap))
                    EnqueueSARS(pred);
        }
        for (int i = 0; i < N; i++) {
            _trainer.SGD(inp[i], outp[i]);
        }
    }

    private void EnqueueSARS(SARS s) {
        var q = _net.Compute(s.State.Features)[_amap[s.Action.ActionId]];
        if (!s.NextState.IsTerminal) {
            var a0max = _net.Compute(s.NextState.Features).Max();
            s.Priority = s.Reward + Discount * a0max - q;
        } else {
            s.Priority = s.Reward - q;
        }
        if (s.Priority > PriorityThreshold)
            _pq.Add(s);
    }

    private void PutPredecessor(SARS sars) {
        if (!_preds.ContainsKey(sars.NextState))
            _preds[sars.NextState] = new List<SARS>();
        var p = _preds[sars.NextState];
        if (!p.Contains(sars))
            p.Add(sars);
    }
}
