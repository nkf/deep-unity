using System.Collections.Generic;
using System.Linq;
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

    private int _size;
    private ConvolutionalNetwork _net;
    private Backprop _trainer;
    private List<SARS> _imitationExps;
    private QExperience _qexp;
    private Dictionary<string, int> _amap;
    private Vector<float> _output;

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
        _trainer = new Backprop(_net, 0.1f, 0.9f);
    }

    public override void SaveModel() {
        _net.Save(MODEL_PATH);
    }

    public override void RemakeModel() {
        Initialize();
        _size = Agent.GetState().Features.RowCount;
        _net = new ConvolutionalNetwork(_size, _amap.Count, new CNNArgs {FilterSize = 3, FilterCount = 3, PoolLayerSize = 2, Stride = 2});
        _trainer = new Backprop(_net, 0.1f, 0.9f);
    }

    public override IEnumerator<YieldInstruction> RunEpisode(QAI.EpisodeCallback callback) {
        Iteration++;
        var s = Agent.GetState();
        while(!s.IsTerminal) {
            // Experience step.
            var a = EpsilonGreedy(Epsilon(Iteration));
            var sars = Agent.MakeSARS(a);
            _qexp.Store(sars, 1000);
            s = sars.NextState;
            // Learning step.
            //TrainModel();
            AdvantageLearning();

            // End of frame.
            yield return new WaitForFixedUpdate();
        }
        callback();
    }
    
    public override ActionValueFunction Q(QState s) {
        _output = _net.Compute(new[] { s.Features });
        Debug.Log(string.Join(";", _output.Select(v => string.Format("{0:.00}", v)).ToArray()) + " ~ " + string.Format("{0:.000}", _output.Average()));
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

    public List<SARS> SampleBatch() {
        //var r = _imitationExps.Random().Concat(_qexp.Random()).ToList();
        //var r = _imitationExps.Concat(_qexp).Shuffle().Take(20).ToList();
        var r = _qexp.Shuffle().Take(10).ToList();
        //var r = _imitationExps.Shuffle().ToList();
        return r;
    }

    private void TrainModel() {
        var batch = SampleBatch();
        var inp = new Matrix<float>[batch.Count];
        var outp = new Vector<float>[batch.Count];
        int i = 0;
        foreach (var sars in batch) {
            inp[i] = sars.State.Features;
            /*
            var ideal = net.Compute(new[] {inp[i]}).Clone();
            float target;
            if(!sars.NextState.IsTerminal) {
                var a0max = _net.Compute(new[] { sars.NextState.Features }).Max();
                target = sars.Reward + Discount * a0max;
            } else {
                target = sars.Reward;
            }
            */
            // ATTENTION: Not Q-learning.
            // Delete from here.
            var ideal = Vector<float>.Build.Dense(3);
            for (int n = 0; n < ideal.Count; n++)
                ideal[n] = 0f;
            var target = 1f;
            // To here.
            ideal[_amap[sars.Action.ActionId]] = target;
            outp[i++] = ideal;
        }
        for (int j = 0; j < batch.Count; j++) {
            _trainer.SGD(new []{inp[j]}, outp[j]);
        }
    }

    private void AdvantageLearning() {
        float dtK = 0.02f;
        float dtKinv = 1 / dtK;
        var batch = SampleBatch();
        foreach (var sars in batch) {
            var inp = new[] { sars.State.Features };
            var outp = _net.Compute(inp).Clone();
            var amax = outp.Max();
            var a0max = _net.Compute(new[] { sars.NextState.Features } ).Max();
            float target = (sars.Reward + Discount * a0max) * dtKinv + (1 - dtKinv) * amax;
            outp[_amap[sars.Action.ActionId]] = target;
            _trainer.SGD(inp, outp);
        }
    }
}
