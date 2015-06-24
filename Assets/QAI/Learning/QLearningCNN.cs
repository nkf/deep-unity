using System;
using System.Collections.Generic;
using System.Linq;
using C5;
using MathNet.Numerics.LinearAlgebra;
using QAI.Agent;
using QAI.Training;
using QNetwork;
using QNetwork.CNN;
using UnityEditor;
using UnityEngine;
using QAI.Visualizer;

namespace QAI.Learning {
    public class QLearningCNN : QLearning {
        public const string MODEL_PATH = "QData/JOHN_N";

        private const float EpisilonStart = 0.5f;
        private const float EpisilonEnd = 0.1f;
        private readonly Param Epsilon = t => EpisilonStart - ((EpisilonEnd - EpisilonStart) / QAIManager.NumIterations()) * t;
        private const float Discount = 0.95f;

        private const bool PrioritySweeping = false;

        private const int BatchSize = 20;
        private const int PredecessorCap = 6;
        private const float PriorityThreshold = 0.01f;

        private readonly BackpropParams LearningParams = new BackpropParams { LearningRate = 0.001f, Momentum = 0.9f };

        private ConvolutionalNetwork _net;
        private List<SARS> _imitationExps;
        private QExperience _qexp;
        private Dictionary<string, int> _amap;
        private Vector<float> _output;

        private readonly Dictionary<QState, List<SARS>> _preds = new Dictionary<QState, List<SARS>>(1000);
        private readonly IntervalHeap<SARS> _pq = new IntervalHeap<SARS>(200, new SARSPrioritizer());

        private QState _prevState;
        private QAction _prevAction;
        private bool _isFirstTurn = true;
        private bool _remake;

        private class SARSPrioritizer : IComparer<SARS> {
            public int Compare(SARS x, SARS y) {
                return (int)(x.Priority - y.Priority);
            }
        }

        private void Initialize(int size) {
            // Action-index mapping.
            _amap = new Dictionary<string, int>();
            int ix = 0;
            foreach(QAction a in Actions)
                _amap[a.ActionId] = ix++;
            // Model.
            if(_remake) {
                _net = new ConvolutionalNetwork(size, _amap.Count, new CNNArgs { FilterSize = 3, FilterCount = 3, PoolLayerSize = 2, Stride = 2 });
            } else {
                _net = ConvolutionalNetwork.Load(MODEL_PATH);
            }
            _net.InitializeTraining(LearningParams);
            // Experience replay.
            LoadExperienceDatabase();
        }

        public override void LoadModel() {
            _remake = false;
            Initialize(0);
        }

        public override void SaveModel() {
            _net.Save(MODEL_PATH);
        }

        public override void RemakeModel() {
            _remake = true;
			Initialize(Agent.GetState().Size);
        }

        public override IEnumerator<YieldInstruction> RunEpisode(QAIManager.EpisodeCallback callback) { throw new NotImplementedException(); }

        public Action GetLearningAction(QState state) {
            if(_net == null) Initialize(state.Size);
            if(!_isFirstTurn) {
                if(state.IsTerminal) {
                    StoreSARS(new SARS(_prevState, _prevAction, state));
                    _isFirstTurn = true;
                    return null;
                }
                if(state.Equals(_prevState)) {
                    return _prevAction.Action;
                }
                StoreSARS(new SARS(_prevState, _prevAction, state));
            }
            var a = EpsilonGreedy(Epsilon(Iteration));
            _prevAction = a;
            _prevState = state;
            _isFirstTurn = false;
            return () => {
                a.Invoke();
                Train();
            };
        }

        private void StoreSARS(SARS sars) {
            if(PrioritySweeping) {
                PutPredecessor(sars);
                EnqueueSARS(sars);
                while(_pq.Count > 100)
                    _pq.DeleteMin();
            } else {
                _qexp.Store(sars, 1000);
            }
        }

        private void Train() {
            if(PrioritySweeping)
                PrioritizedSweeping();
            else
                TrainModel();
        }

        public override ActionValueFunction Q(QState s) {
            _output = _net.Compute(s.Features);
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
            foreach(var imitationExp in _imitationExps) {
                _qexp.Store(imitationExp);
                PutPredecessor(imitationExp);
                EnqueueSARS(imitationExp);
            }
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
            foreach(var sars in batch) {
                inp[i] = sars.State.Features;
                float target;
                if(!sars.NextState.IsTerminal) {
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
            for(int j = 0; j < batch.Count; j++) {
                _net.SGD(inp[j], outp[j]);
            }
        }

        private void PrioritizedSweeping() {
            int N = Mathf.Min(BatchSize, _pq.Count);
            var inp = new Matrix<float>[N][];
            var outp = new TargetIndexPair[N];
            for(int i = 0; i < N; i++) {
                var sars = _pq.DeleteMax();
                inp[i] = sars.State.Features;
                float target;
                if(!sars.NextState.IsTerminal) {
                    var a0max = _net.Compute(sars.NextState.Features).Max();
                    target = sars.Reward + Discount * a0max;
                } else {
                    target = sars.Reward;
                }
                outp[i] = new TargetIndexPair(target, _amap[sars.Action.ActionId]);
                if(_preds.ContainsKey(sars.State))
                    foreach(var pred in _preds[sars.State].Shuffle().Take(PredecessorCap))
                        EnqueueSARS(pred);
            }
            for(int i = 0; i < N; i++) {
                _net.SGD(inp[i], outp[i]);
            }
        }

        private void EnqueueSARS(SARS s) {
            var q = _net.Compute(s.State.Features)[_amap[s.Action.ActionId]];
            if(!s.NextState.IsTerminal) {
                var a0max = _net.Compute(s.NextState.Features).Max();
                s.Priority = s.Reward + Discount * a0max - q;
            } else {
                s.Priority = s.Reward - q;
            }
            if(s.Priority > PriorityThreshold)
                _pq.Add(s);
        }

        private void PutPredecessor(SARS sars) {
            if(!_preds.ContainsKey(sars.NextState))
                _preds[sars.NextState] = new List<SARS>();
            var p = _preds[sars.NextState];
            if(!p.Contains(sars))
                p.Add(sars);
        }

		public NetworkVisualizer CreateVisualizer() {
			return NetworkVisualizer.CreateVisualizer(_net);
		}
    }

}
