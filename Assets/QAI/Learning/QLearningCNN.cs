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

        private readonly BackpropParams LearningParams = new BackpropParams { LearningRate = 0.005f, Momentum = 0.9f, Decay = 0.0f };

        private ConvolutionalNetwork _net;
        private Dictionary<string, int> _amap;
        private Vector<float> _output;

        private bool _remake;

		public QLearningCNN(bool PrioritizedSweeping) {
			PrioritySweeping = PrioritizedSweeping;
			BatchSize = PrioritySweeping ? 5 : 100;
			MaxStoreSize = PrioritySweeping ? 30 : 2000;
		}

        public override void Initialize(int gridSize, int vectorSize, int depth) {
            // Action-index mapping.
            _amap = new Dictionary<string, int>();
            int ix = 0;
            foreach (QAction a in Actions)
                _amap[a.ActionId] = ix++;
            // Model.
            if (_remake) {
                _net = new ConvolutionalNetwork(gridSize, vectorSize, depth, _amap.Count,
                    //new CNNArgs { FilterSize = 3, FilterCount = 3, PoolLayerSize = 2, Stride = 2 },
                    new CNNArgs { FilterSize = 4, FilterCount = 1, PoolLayerSize = 2, Stride = 1 });
            } else {
                _net = ConvolutionalNetwork.Load(BenchmarkSave.ModelPath);
            }
            _net.InitializeTraining(LearningParams);
            // Experience replay.
            LoadExperienceDatabase();
        }

        public override void LoadModel() {
            _remake = false;
            Initialize(0, 0, 0);
        }

        public override void SaveModel() {
            _net.Save(BenchmarkSave.ModelPath);
        }

        public override void RemakeModel(QState exampleState) {
            _remake = true;
			Initialize(exampleState.GridSize, exampleState.VectorSize, exampleState.Depth);
        }

        public override bool ModelReady() {
            return _net != null;
        }

        public override ActionValueFunction Q(QState s) {
            _output = _net.Compute(s.Features);
            return a => _output[_amap[a.ActionId]];
        }

        public override float QMax(QState s) {
            return _net.Compute(s.Features).Max();
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
