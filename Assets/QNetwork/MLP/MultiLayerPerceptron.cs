using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.MLP {
    public class MultiLayerPerceptron : Network<Vector<float>> {
        private InputLayer input;
        private DenseLayer[] hidden;
        private DenseLayer output;

        public MultiLayerPerceptron(int insize, params int[] layers) {
            input = new InputLayer(insize);
            hidden = new DenseLayer[layers.Length - 1];
            hidden[0] = new DenseLayer(layers[0], input, Functions.Sigmoid);
            for (int i = 1; i < hidden.Length; i++)
                hidden[i] = new DenseLayer(layers[i], hidden[i - 1], Functions.Sigmoid);
            output = new DenseLayer(layers[layers.Length - 1], hidden[hidden.Length - 1], Functions.Sigmoid);
        }

        public int Size() {
            return hidden.Length + 2;
        }

        public Vector<float> Compute(Vector<float> input) {
            return BottomUp().ForwardPropagation(input);
        }

        public Vector<float> Output() {
            return output.Output();
        }

        public T Accept<T>(Trainer<T> t, T state) {
            return TopDown().ApplyTrainer(t, state);
        }

        public IEnumerable<Unit<Vector<float>>> BottomUp() {
            yield return input;
            for (int i = 0; i < hidden.Length; i++)
                yield return hidden[i];
            yield return output;
        }

        public IEnumerable<Unit<Vector<float>>> TopDown() {
            yield return output;
            for (int i = hidden.Length - 1; i >= 0; i--)
                yield return hidden[i];
            yield return input;
        }
    }
}
