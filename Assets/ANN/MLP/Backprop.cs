using System;
using System.Collections.Generic;
using System.Linq;

namespace QNetwork.MLP {
    public struct BackpropStatus {
        public int iterations;
        public float meanSquaredError;
        public float meanWeightChange;
    }

	public class Backprop : Trainer<DataPair<float>, BackpropStatus> {
        private BackpropStatus status = new BackpropStatus { iterations = 1 };
        private LearningRate<BackpropStatus> lrate = s => 1f;
        private TerminationCondition<BackpropStatus> terminate = s =>
            s.iterations > 1000 || s.meanSquaredError < 0.1f;

        private const float MOMENTUM = 0.9f;
        private const float DECAY = 0.0f;

        private Network net;
        private int outputs;
        private int weights;

        public Backprop(Network net) {
            this.net = net;
            outputs = net.Last().Size;
            weights = net.SelectMany(l => l.Select(n => n.Count())).Sum();
        }

        public void PassOne(DataPair<float> tuple) {
            net.Compute(tuple.Input);
            var errors = net.Output().Select((y, i) => tuple.Targets.ElementAt(i) - y).ToArray();
            status.meanSquaredError += errors.Select(e => e * e).Sum();
            net.Last().ForEach((n, i) => GradientDescent(n, errors[i]));
            net.Reverse().Skip(1).ForEach(l => l.ForEach(n => GradientDescent(n, n.Select(c => c.Neuron.Error * c.Weight).Sum())));
        }

        public void PassAll(IEnumerable<DataPair<float>> data) {
            int count = data.Count();
            data.Shuffle().ForEach(t => PassOne(t));
            status.meanSquaredError /= outputs * count;
            status.meanWeightChange /= weights * count;
            status.iterations++;
        }

        public void Train(IEnumerable<DataPair<float>> data) {
            do {
                status.meanSquaredError = 0f;
                status.meanWeightChange = 0f;
                PassAll(data);
            } while (!terminate(status));
            UnityEngine.Debug.Log("Iterations: " + status.iterations);
            UnityEngine.Debug.Log("Mean squared errors: " + status.meanSquaredError);
            UnityEngine.Debug.Log("Mean delta weight: " + status.meanWeightChange);
            Reset();
        }

        public void Reset() {
            status = new BackpropStatus { iterations = 1 };
        }

        public void SetLearningRate(LearningRate<BackpropStatus> rate) {
            lrate = rate;
        }

        public void SetTerminationCondition(TerminationCondition<BackpropStatus> condition) {
            terminate = condition;
        }

        private void GradientDescent(Neuron n, float error) {
            n.Error = n.Function.dy(n.Value) * error;
            n.ForEach(c => {
                float dw = c.Neuron.Error * n.Value * lrate(status);
                float ext = c.PrevDeltaWeight * MOMENTUM - c.Weight * DECAY;
                status.meanWeightChange += Math.Abs(dw + ext);
                c.AdjustWeight(dw, ext);
            });
            n.Bias += n.Error * lrate(status);
        }
	}
}
