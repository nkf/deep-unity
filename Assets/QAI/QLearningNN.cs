using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using Encog.Engine.Network.Activation;
using Encog.ML.Data.Basic;
using UnityEngine;
using Random = System.Random;

public class QLearningNN : QLearning {
    public const string MODEL_PATH = "QData/JOHN_N.xml";
    public const string IMITATION_PATH = "QData/Imitation/Assets_main-0.xml";
    public const double DOMAIN = 5.0;

    private Param Epsilon = t => 25 - t / 4;
    private double Discount = 0.99;

    private BasicNetwork _net;
    private QExperience _exp;
    private Dictionary<string, int> _amap;
    private double[] _output;
    private readonly bool _imit;

    public override void LoadModel() {
        RemakeModel();
        var xml = new XmlSerializer(typeof(double[]));
        using (var fs = File.Open(MODEL_PATH, FileMode.Open)) {
            var data = (double[])xml.Deserialize(fs);
            _net.DecodeFromArray(data);
        }
    }

    public override void SaveModel() {
        var xml = new XmlSerializer(typeof(double[]));
        using (var fs = File.Open(MODEL_PATH, FileMode.Create)) {
            double[] data = new double[_net.EncodedArrayLength()];
            _net.EncodeToArray(data);
            xml.Serialize(fs, data);
        }
    }

    public override void RemakeModel() {
        LoadExperienceDatabase();
        // Action-index mapping.
        _amap = new Dictionary<string, int>();
        int ix = 0;
        foreach (QAction a in Actions)
            _amap[a.ActionId] = ix++;
        // Network architecture.
        int size = Agent.GetState().Features.Length;
        _net = new BasicNetwork();
        _net.AddLayer(new BasicLayer(null, true, size));
        _net.AddLayer(new BasicLayer(new ActivationSigmoid(), true, size * 2));
        _net.AddLayer(new BasicLayer(new ActivationSigmoid(), false, Actions.Count));
        _net.Structure.FinalizeStructure();
        _net.Reset();
    }

    public override IEnumerator<YieldInstruction> RunEpisode(QAI.EpisodeCallback callback) {
        Iteration++;
        var s = Agent.GetState();
        while (!s.IsTerminal) {
            // Experience step.
            var a = EpsilonGreedy(Epsilon(Iteration));
            _exp.Store(Agent.MakeSARS(a));
            // Learning step.
            TrainModel();
            // End of frame.
            yield return new WaitForEndOfFrame();
        }
        callback();
    }

    public override ActionValueFunction Q(QState s) {
        _output = _net.Compute(new BasicMLData(Agent.GetState().Features)).ToEnumerable().ToArray();
        return a => _output[_amap[a.ActionId]];
    }

    public void LoadExperienceDatabase() {
        _exp = new QExperience(); // TODO
    }

    public IEnumerable<SARS> SampleBatch() {
        return _exp.Shuffle(); // TODO
    }

    private void TrainModel() {
        var dlist = new List<Encog.ML.Data.IMLDataPair>();
        foreach (var sars in SampleBatch()) {
            var inp = new BasicMLData(sars.State.Features);
            var ideal = _net.Compute(inp).ToEnumerable().ToArray();
            double target;
            if (!sars.NextState.IsTerminal) {
                var inp0 = new BasicMLData(sars.NextState.Features);
                var a0max = _net.Compute(inp0).ToEnumerable().Max();
                target = sars.Reward + Discount + a0max;
            } else {
                target = sars.Reward;
            }
            ideal[_amap[sars.Action.ActionId]] = target;
            dlist.Add(new BasicMLDataPair(inp, new BasicMLData(ideal)));
        }
        var train = new ResilientPropagation(_net, new BasicMLDataSet(dlist));
        train.Iteration(dlist.Count);
        train.FinishTraining();
    }
}
