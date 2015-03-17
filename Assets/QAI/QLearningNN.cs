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

public class QLearningNN {
    private delegate double ActionValueFunction(QAction a);
    
    public const double TIE_BREAK = 1e-9;
    public const string MODEL_PATH = "QData/JOHN_N.xml";
    public const string IMITATION_PATH = "QData/Imitation/Assets_main-0.xml";
    public const double DOMAIN = 5.0;

    private QLearning.param Epsilon = t => 25 - t / 4;
    private QLearning.param Discount = t => 0.99;
    private QLearning.param StepSize = t => 1.0 / t;

    public QAgent Agent { get; private set; }
    public IList<QAction> Actions { get; private set; }
    public int Iteration { get; private set; }

    private BasicNetwork _net;
    private QExperience _exp;
    private Dictionary<string, int> _amap;
    private double[] _output;
    private readonly bool _imit;
    private readonly Random _rng;

    public QLearningNN(QAgent agent, bool imitating = false) {
        _rng = new Random();
        _imit = imitating;
        Agent = agent;
        Actions = agent.GetQActions();
        _exp = QExperience.Load(IMITATION_PATH);
    }

    public void LoadModel() {
        //_net = QNetwork.Load(MODEL_PATH);
        var xml = new XmlSerializer(typeof(double[]));
        using (var fs = File.Open(MODEL_PATH, FileMode.Open)) {
            var data = (double[])xml.Deserialize(fs);
            RemakeModel();
            _net.DecodeFromArray(data);
        }
    }

    public void SaveModel() {
        //_net.Save(MODEL_PATH);
        var xml = new XmlSerializer(typeof(double[]));
        using (var fs = File.Open(MODEL_PATH, FileMode.Create)) {
            double[] data = new double[_net.EncodedArrayLength()];
            _net.EncodeToArray(data);
            xml.Serialize(fs, data);
        }
    }

    public void RemakeModel() {
        int size = Agent.GetState().Features.Length;
        _net = new BasicNetwork();
        _net.AddLayer(new BasicLayer(null, true, size));
        _net.AddLayer(new BasicLayer(new ActivationSigmoid(), true, size * 2));
        _net.AddLayer(new BasicLayer(new ActivationSigmoid(), false, Actions.Count));
        _net.Structure.FinalizeStructure();
        _net.Reset();
        //_net = new QNetwork(size, Actions.Count, 1, size * 2);
        _amap = new Dictionary<string, int>();
        int ix = 0;
        foreach (QAction a in Actions)
            _amap[a.ActionId] = ix++;
    }

    public IEnumerator<YieldInstruction> RunEpisode(QAgent agent, QAI.EpisodeCallback callback) {
        /*Iteration++;
        Agent = agent;
        Actions = agent.GetQActions();
        var s = Agent.GetState();
        while (!sars.State.IsTerminal) {
        for (int i = 0; i < 1000; i++) {
            foreach (var sars in _exp) {
                //var a = _imit ? Agent.ConvertImitationAction() : Policy(s);
                //sars.Action.Invoke();
                //var s0 = Agent.GetState();
                if (!sars.NextState.IsTerminal) {
                    var q0 = Q(sars.NextState);
                    var a0max = Actions.Max(a0 => q0(a0));
                    var target = sars.Reward + Discount(Iteration) * a0max;
                    Q(sars.State);
                    Update(sars.State, sars.Action, target);
                    //s = s0;
                } else {
                    Update(sars.State, sars.Action, sars.Reward);
                    //break;
                }
            }
        }*/
        Iteration++;
        var dlist = new List<Encog.ML.Data.IMLDataPair>();
        foreach (var sars in _exp) {
            var inp = new BasicMLData(sars.State.Features.Select(f => (double)f).ToArray());
            var inp0 = new BasicMLData(sars.NextState.Features.Select(f => (double)f).ToArray());
            var ideal = Iter(_net.Compute(inp)).ToArray();
            var a0max = Iter(_net.Compute(inp0)).Max();
            var target = sars.Reward + Discount(Iteration) + a0max;
            ideal[_amap[sars.Action.ActionId]] = target;
            dlist.Add(new BasicMLDataPair(inp, new BasicMLData(ideal)));
        }
        var train = new ResilientPropagation(_net, new BasicMLDataSet(dlist));
        train.Iteration(dlist.Count);
        train.FinishTraining();
        yield return new WaitForEndOfFrame();
        callback();
    }

    private ActionValueFunction Q(QState s) {
        //_net.Feedforward(s.Features.Select(f => (double)f).Normalize(DOMAIN).ToArray());
        //_output = _net.Output().ToArray();
        var res = _net.Compute(new BasicMLData(Agent.GetState().Features.Select(f => (double)f).ToArray()));
        _output = Iter(res).ToArray();
        return a => _output[_amap[a.ActionId]];
    }

    private void Update(QState s, QAction a, double v) {
        //_output[_amap[a.ActionId]] = v;
        //_net.Backpropagate(_output, StepSize(Iteration));
        throw new NotImplementedException();
    }

    private QAction Policy(QState s) {
        throw new NotImplementedException();
        //var q = Q(s);
        //if (_rng.NextDouble() < Epsilon(Iteration)) return Actions[_rng.Next(Actions.Count)];
        //return Actions.Where(a => a.IsValid()).OrderByDescending(a => q(a) + _rng.NextDouble() * TIE_BREAK).First();
    }

    public QAction BestAction() {
        var s = Agent.GetState();
        var q = Q(s);
        return Actions.Where(a => a.IsValid()).OrderByDescending(a => q(a)).First();

    }

    private IEnumerable<double> Iter(Encog.ML.Data.IMLData data) {
        for (int i = 0; i < data.Count; i++)
            yield return data[i];
    }
}
