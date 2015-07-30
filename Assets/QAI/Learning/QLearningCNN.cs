using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using QAI.Agent;
using QAI.Training;
using QAI.Visualizer;
using QNetwork;
using QNetwork.CNN;

namespace QAI.Learning {
    public class QLearningCNN : QLearning {

        private readonly BackpropParams LearningParams;

        private ConvolutionalNetwork _net;
		public ConvolutionalNetwork CNN { get { return _net; }}
        private Dictionary<string, int> _amap;
        private Vector<float> _output;

        private bool _remake;
        private CNNArgs[] _networkArgs;

		public QLearningCNN(bool prioritizedSweeping, QOption option) {
			PrioritySweeping = prioritizedSweeping;
			Discretize = option.Discretize;
			TrainingInterval = option.TrainingInterval;
			TrainingCycles = option.TrainingCycle;
			EpsilonStart = option.EpsilonStart;
			EpsilonEnd = option.EpsilonEnd;
			Discount = option.Discount;
			BatchSize = PrioritySweeping ? 5 : option.BatchSize;
			MaxStoreSize = PrioritySweeping ? 30 : option.MaxPoolSize;
            LearningParams = new BackpropParams { LearningRate = option.LearningRate, Momentum = 0.9f, Decay = 0.0f };
		    _networkArgs = option.NetworkArgs;
		}

        public override void Initialize(int gridSize, int vectorSize) {
            // Action-index mapping.
            _amap = new Dictionary<string, int>();
            int ix = 0;
            foreach (QAction a in Actions)
                _amap[a.ActionId] = ix++;
            // Model.
            if (_remake) {
                _net = new ConvolutionalNetwork(gridSize, vectorSize, _amap.Count, _networkArgs);
            } else {
                _net = ConvolutionalNetwork.Load(BenchmarkSave.ModelPath);
            }
            _net.InitializeTraining(LearningParams);
            // Experience replay.
            LoadExperienceDatabase();
        }

        public override void LoadModel() {
            _remake = false;
            Initialize(0, 0);
        }

        public override void SaveModel() {
            _net.Save(BenchmarkSave.ModelPath);
        }

        public override void RemakeModel(QState exampleState) {
            _remake = true;
			Initialize(exampleState.GridSize, exampleState.VectorSize);
        }

        public override bool ModelReady() {
            return _net != null;
        }

        public override ActionValueFunction Q(QState s) {
            _output = _net.Compute(s.Features);
            //Debug.Log(string.Join(";", _output.Select(v => string.Format("{0:.00}", v)).ToArray()) + " ~ " + string.Format("{0:.000}", _output.Average()));
            return a => _output[_amap[a.ActionId]];
        }

        public override float QMax(QState s) {
            return _net.Compute(s.Features, true).Max();
        }

        public override void TrainModel(List<SARS> batch) {
            var inp = new StatePair[batch.Count];
            var outp = new TargetIndexPair[batch.Count];
            int i = 0;
            foreach(var sars in batch) {
                inp[i] = sars.State.Features;
                float target;
                if(!sars.NextState.IsTerminal) {
                    var a0max = QMax(sars.NextState);
                    target = sars.Reward + Discount * a0max;
                } else {
                    target = sars.Reward;
                }
                outp[i++] = new TargetIndexPair(target, _amap[sars.Action.ActionId]);
            }
            for(int j = 0; j < batch.Count; j++) {
                _net.SGD(inp[j], outp[j]);
            }
        }

		public override NetworkVisualizer CreateVisualizer() {
		    var list = _amap.Keys.ToList();
            list.Sort((s1,s2) => _amap[s1]-_amap[s2]);
			return NetworkVisualizer.CreateVisualizer(_net, list.ToArray());
		}
    }
}
