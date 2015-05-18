using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using QNetwork.Training;

namespace QNetwork.MLP {
    public class DenseLayer : Layer {
        private Vector<float> values;
        public ActivationFunction<Vector<float>> Activation { get; set; }
        public Vector<float> Biases { get; set; }
        public Matrix<float> Weights { get; set; }

        public DenseLayer(int size, Layer prev, ActivationFunction<Vector<float>> activation) {
            Activation = activation;
            var vb = Vector<float>.Build;
            var mb = Matrix<float>.Build;
            values = vb.Dense(size);
            Biases = vb.Random(size, Normal.WithMeanStdDev(0.0, 0.05));
            Weights = mb.Random(size, prev.Size(), Normal.WithMeanStdDev(0.0, 0.5 / Math.Sqrt(prev.Size())));
            Prev = prev;
            prev.Next = this;
        }

        public override int Size() {
            return Weights.RowCount;
        }

        public override Vector<float> Compute(Vector<float> input) {
            Weights.Multiply(input, values);
            values.Add(Biases, values);
            Activation.Apply(values, values);
            return values;
        }

        public override Vector<float> Output() {
            return values;
        }

        public override T Accept<T>(Trainer<T> t, T state) {
            return t.Visit(this, state);
        }
    }
}
