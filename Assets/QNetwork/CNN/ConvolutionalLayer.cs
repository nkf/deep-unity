using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using QNetwork.Training;

namespace QNetwork.CNN {
	public class ConvolutionalLayer : SpatialLayer {
        public ActivationFunction<Matrix<float>> Activation;
        public Vector<float> Biases { get; set; }
        public Matrix<float>[][] Weights { get; set; }
        public new SpatialLayer Prev { get; set; }
        public int Stride { get; set; }

        private Matrix<float> cache, buffer;

        public ConvolutionalLayer(int fsize, int numf, int stride, SpatialLayer prev, ActivationFunction<Matrix<float>> activation)
            : base((prev.SideLength - fsize) / stride + 1, numf) {
            Activation = activation;
            var vb = Vector<float>.Build;
            var mb = Matrix<float>.Build;
            values = new Matrix<float>[ChannelCount];
            for (int i = 0; i < values.Length; i++)
                values[i] = Matrix<float>.Build.Dense(SideLength, SideLength);
            Biases = vb.Random(ChannelCount, Normal.WithMeanStdDev(0.0, 0.05));
            Weights = new Matrix<float>[prev.ChannelCount][];
            for (int i = 0; i < Weights.Length; i++) {
                Weights[i] = new Matrix<float>[ChannelCount];
                for (int j = 0; j < Weights[i].Length; j++)
                    Weights[i][j] = mb.Random(fsize, fsize, Normal.WithMeanStdDev(0.0, 0.5 / (SideLength * SideLength)));
            }
            Prev = prev;
            prev.Next = this;
            Stride = stride;
            cache = mb.Dense(fsize, fsize);
            if (prev.ChannelCount > 1) // No buffer needed if there is only 1 input channel.
                buffer = mb.Dense(SideLength, SideLength);
        }

        public override Matrix<float>[] Compute(Matrix<float>[] input) {
            // Input from first channel. No buffer needed (values are overwritten).
            for (int j = 0; j < Weights[0].Length; j++) {
                Convolution(input[0], Weights[0][j], values[j]);
            }
            // Input from remaining channels. Use of buffer in order to accumulate values.
            for (int i = 1; i < Weights.Length; i++) {
                for (int j = 0; j < Weights[i].Length; j++) {
                    Convolution(input[i], Weights[i][j], buffer);
                    values[j].Add(buffer, values[j]);
                }
            }
            // Add biases and apply activation function.
            for (int i = 0; i < values.Length; i++) {
                values[i].Add(Biases.At(i), values[i]);
                Activation.Apply(values[i], values[i]);
            }
            return values;
        }

        private void Convolution(Matrix<float> source, Matrix<float> filter, Matrix<float> dest) {
            for (int m = 0; m < dest.RowCount; m += Stride)
                for (int n = 0; n < dest.ColumnCount; n += Stride) {
                    source.SubMatrix(m, filter.RowCount, n, filter.ColumnCount).PointwiseMultiply(filter, cache);
                    dest.At(m, n, cache.RowSums().Sum());
                }
        }

        public override T Accept<T>(Trainer<T> t, T state) {
            return t.Visit(this, state);
        }
	}
}
