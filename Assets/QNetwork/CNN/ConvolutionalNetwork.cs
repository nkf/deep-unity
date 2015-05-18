using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using QNetwork.MLP;

namespace QNetwork.CNN {
	public class ConvolutionalNetwork : Network<Matrix<float>[]> {
        private SpatialLayer input;
        private SpatialLayer[] hidden;
        private DenseLayer output;

        private Matrix<float>[] result;

        public ConvolutionalNetwork(int inroot, int labels, params int[][] convl) {
            input = new SpatialLayer(inroot, 1); // TODO: Channels.
            hidden = new SpatialLayer[convl.Length * 2];
            hidden[0] = new ConvolutionalLayer(convl[0][0], convl[0][1], convl[0][2], input, Functions.Tanh2D);
            hidden[1] = new MeanPoolLayer(convl[0][3], hidden[0]);
            for (int i = 2; i < hidden.Length; i += 2) {
                hidden[i] = new ConvolutionalLayer(convl[i][0], convl[i][1], convl[i][2], hidden[i - 1], Functions.Tanh2D);
                hidden[i + 1] = new MeanPoolLayer(convl[i][3], hidden[i]);
            }
            output = new DenseLayer(labels, hidden[hidden.Length - 1], Functions.Softmax);
            result = new Matrix<float>[1];
            result[0] = Matrix<float>.Build.Dense(1, output.Size());
        }

        public int Size() {
            return (hidden.Length / 2) + 2;
        }

        public Matrix<float>[] Compute(Matrix<float>[] input) {
            BottomUp().ForwardPropagation(input);
            result[0].SetRow(0, output.Compute(hidden[hidden.Length - 1].Output()));
            return result;
        }

        public Matrix<float>[] Output() {
            return result;
        }

        public T Accept<T>(Trainer<T> t, T state) {
            return TopDown().ApplyTrainer(t, output.Accept(t, state));
        }

        public IEnumerable<Unit<Matrix<float>[]>> BottomUp() {
            yield return input;
            for (int i = 0; i < hidden.Length; i++)
                yield return hidden[i];
        }

        public IEnumerable<Unit<Matrix<float>[]>> TopDown() {
            for (int i = hidden.Length - 1; i >= 0; i--)
                yield return hidden[i];
            yield return input;
        }
	}
}
