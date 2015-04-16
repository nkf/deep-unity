using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Accord.Math;
using Accord.Neuro;
using Accord.Neuro.ActivationFunctions;
using Accord.Neuro.Learning;
using Accord.Neuro.Networks;
using AForge.Neuro;
using AForge.Neuro.Learning;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

public class QLearningNN : QLearning {
    public const string MODEL_PATH = "QData/JOHN_N";

    private Param Epsilon = t => 0.5 - (0.5 / QAI.NumIterations()) * t;
    private double Discount = 0.9;

    private const int complexity = 1;
    private int size;
    private DeepBeliefNetwork[] _nets = new DeepBeliefNetwork[complexity];
    private List<QExperience> _exps;
    private QExperience _qexp;
    private Dictionary<string, int> _amap;
    private double[] _output;
    private readonly bool _imit;

    private void Initialize() {
        LoadExperienceDatabase();
        // Action-index mapping.
        _amap = new Dictionary<string, int>();
        int ix = 0;
        foreach (QAction a in Actions)
            _amap[a.ActionId] = ix++;
    }

    public override void LoadModel() {
        Initialize();
        for (int n = 0; n < _nets.Length; n++)
            _nets[n] = DeepBeliefNetwork.Load(MODEL_PATH + "" + n);
    }

    public override void SaveModel() {
        for (int n = 0; n < _nets.Length; n++)
            _nets[n].Save(MODEL_PATH + "" + n);
    }

    public override void RemakeModel() {
        Initialize();
        // Deep belief network.
        size = Agent.GetState().Features.Length / complexity;
        for (int n = 0; n < _nets.Length; n++) {
            _nets[n] = new DeepBeliefNetwork(size, size * 3, Actions.Count);
            new GaussianWeights(_nets[n]).Randomize();
            _nets[n].UpdateVisibleWeights();
        }
        PreTrain();
    }

    public override IEnumerator<YieldInstruction> RunEpisode(QAI.EpisodeCallback callback) {
        Iteration++;
        var s = Agent.GetState();
        while (!s.IsTerminal) {
            // Experience step.
            var a = EpsilonGreedy(Epsilon(Iteration));
            var sars = Agent.MakeSARS(a);
            _qexp.Store(sars, 20);
            s = sars.NextState;
            // Learning step.
            TrainModel();
            // End of frame.
            yield return new WaitForFixedUpdate();
        }
        callback();
    }

    public override ActionValueFunction Q(QState s) {
        _output = _nets[0].Compute(s.Features);
        //Debug.Log(string.Join(";", _output.Select(v => string.Format("{0:.00}", v)).ToArray()));
        return a => _output[_amap[a.ActionId]];
    }

    public void LoadExperienceDatabase() {
		_exps = QStory.LoadAll("QData/Story")
            .Where(qs => qs.ScenePath == EditorApplication.currentScene)
            .SelectMany(qs => qs.ImitationExperiences.Select(qi => qi.Experience)).ToList();
		Debug.Log ("Loading " + _exps.Count + " imitation experiences");
        _qexp = new QExperience();
    }

    public IEnumerable<SARS> SampleBatch() {
        // TODO: This line has a huge impact on learning ability. Change as needed.
        return _exps.SelectMany(e => e).Concat(_qexp).Shuffle().Take(100);
    }

    private void PreTrain() {
        for (int n = 0; n < _nets.Length; n++) {
            var inp = _exps.SelectMany(e => e).Concat(_qexp).Shuffle().Select(
                s => s.State.Features.Skip(n * size).Take(size).ToArray()
            ).ToArray();
            var trainer = new DeepBeliefNetworkLearning(_nets[n]);
            trainer.Algorithm = (h, v, j) => new ContrastiveDivergenceLearning(h, v);
            for (int i = 0; i < _nets[n].Machines.Count - 1; i++) {
                trainer.LayerIndex = i;
                var data = trainer.GetLayerInput(inp);
                trainer.RunEpoch(data);
            }
        }
    }

    private void TrainModel() {
        var batch = SampleBatch().ToList();
        var inp = new double[batch.Count][];
        var outp = new double[batch.Count][];
        int i = 0;
        for (int n = 0; n < _nets.Length; n++) {
            foreach (var sars in batch) {
                inp[i] = sars.State.Features;
                var ideal = _nets[n].Compute(inp[i]);
                double target;
                if (!sars.NextState.IsTerminal) {
                    var a0max = _nets[n].Compute(sars.NextState.Features).Max();
                    target = sars.Reward + Discount * a0max;
                } else {
                    target = sars.Reward;
                }
                ideal[_amap[sars.Action.ActionId]] = target;
                outp[i++] = ideal;
            }
            var backprop = new BackPropagationLearning(_nets[n]);
            backprop.RunEpoch(inp, outp);
        }
    }
}
