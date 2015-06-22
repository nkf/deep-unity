using System.Collections.Generic;
using System.IO;
using System.Linq;
using Accord.Neuro;
using Accord.Neuro.Learning;
using Accord.Neuro.Networks;
using AForge.Neuro;
using AForge.Neuro.Learning;
using QAI.Agent;
using QAI.Training;
using UnityEditor;
using UnityEngine;
using Network = AForge.Neuro.Network;

namespace QAI.Learning {
    public class QLearningDBN : QLearning {
        public const string MODEL_PATH = "QData/JOHN_N";

        private const float EpisilonStart = 0.5f;
        private const float EpisilonEnd = 0.1f;
        private readonly Param Epsilon = t => EpisilonStart - ((EpisilonEnd-EpisilonStart) / QAIManager.NumIterations()) * t;
        private const double Discount = 0.95;

        private const int Complexity = 10;
        private const int NetworkCacheDuration = 1;
        private int size;
        private DeepBeliefNetwork _net;
        private DeepBeliefNetwork _netRecent;
        private List<SARS> _imitationExps;
        private QExperience _qexp;
        private Dictionary<string, int> _amap;
        private double[] _output;
        private readonly bool _imit;

        private void Initialize() {
            LoadExperienceDatabase();
            // Action-index mapping.
            _amap = new Dictionary<string, int>();
            int ix = 0;
            foreach (var a in Actions)
                _amap[a.ActionId] = ix++;
        }

        public override void LoadModel() {
            Initialize();
            _net = DeepBeliefNetwork.Load(MODEL_PATH);
            _netRecent = Clone(_net);
        }

        public override void SaveModel() {
            _netRecent.Save(MODEL_PATH);
        }

        public override void RemakeModel() {
            Initialize();
            // Deep belief network.
//        size = Agent.GetState().Features.Length;
            _net = new DeepBeliefNetwork(size, size * 3, Actions.Count);
            new GaussianWeights(_net).Randomize();
            _net.UpdateVisibleWeights();
            PreTrain();
            _netRecent = Clone(_net);
        }

        public override IEnumerator<YieldInstruction> RunEpisode(QAIManager.EpisodeCallback callback) {
            Iteration++;
            var s = Agent.GetState();
            var n = 0;
            while (!s.IsTerminal) {
                // Experience step.
                var a = EpsilonGreedy(Epsilon(Iteration));
                var sars = Agent.MakeSARS(a);
                _qexp.Store(sars, 100);
                s = sars.NextState;
                // Learning step.
                TrainModel(_net, _netRecent);

                //Cache and swap network
                if (++n > NetworkCacheDuration) {
                    n = 0;
                    _net = _netRecent;
                    _netRecent = Clone(_net);
                }
                // End of frame.
                yield return new WaitForFixedUpdate();
            }
            callback();
        }

        public override ActionValueFunction Q(QState s) {
//        _output = _netRecent.Compute(s.Features);
            //Debug.Log(string.Join(";", _output.Select(v => string.Format("{0:.00}", v)).ToArray()) + " ~ " + string.Format("{0:.000}",_output.Average()));
            return a => _output[_amap[a.ActionId]];
        }

        public void LoadExperienceDatabase() {
            _imitationExps = QStory.LoadAll("QData/Story")
                .Where(qs => qs.ScenePath == EditorApplication.currentScene)
                .SelectMany(qs => qs.ImitationExperiences.Select(qi => qi.Experience))
                .SelectMany(e => e).ToList();
            Debug.Log ("Loading " + _imitationExps.Count + " imitation experiences");
            _qexp = new QExperience();
        }

        public List<SARS> SampleBatch() {
            //var r = _imitationExps.Random().Concat(_qexp.Random()).ToList();
            var r = _imitationExps.Concat(_qexp).Shuffle().Take(15).ToList();
            //var r = _qexp.Shuffle().Take(15).ToList();
            return r;
        }

        private void PreTrain() {
            var inp = _imitationExps.Concat(_qexp).Shuffle().Select(
                s => s.State.Features
                ).ToArray();
            var trainer = new DeepBeliefNetworkLearning(_net);
            trainer.Algorithm = (h, v, j) => new ContrastiveDivergenceLearning(h, v);
            for (int i = 0; i < _net.Machines.Count - 1; i++) {
                trainer.LayerIndex = i;
//            var data = trainer.GetLayerInput(inp);
//            trainer.RunEpoch(data);
            }
        }

        private void TrainModel(Network query, ActivationNetwork train) {
            var batch = SampleBatch();
            var inp = new double[batch.Count][];
            var outp = new double[batch.Count][];
            int i = 0;
            foreach (var sars in batch) {
//            inp[i] = sars.State.Features;
                var ideal = query.Compute(inp[i]);
                double target;
                if (!sars.NextState.IsTerminal) {
//                var a0max = query.Compute(sars.NextState.Features).Max();
//                target = sars.Reward + Discount * a0max;
                } else {
                    target = sars.Reward;
                }
//            ideal[_amap[sars.Action.ActionId]] = target;
                outp[i++] = ideal;
            }
            var backprop = new BackPropagationLearning(train);
            backprop.RunEpoch(inp, outp);
        }

        private DeepBeliefNetwork Clone(DeepBeliefNetwork network) {
            using (var stream = new MemoryStream()) {
                network.Save(stream);
                stream.Position = 0;
                return DeepBeliefNetwork.Load(stream);
            }
        }
    }
}
