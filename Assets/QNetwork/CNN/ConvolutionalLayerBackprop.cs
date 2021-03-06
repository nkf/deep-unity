﻿using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.CNN {
	public class ConvolutionalLayerBackprop : Backprop<Matrix<float>[], Matrix<float>[]> {
        private readonly ConvolutionalLayer _unit;
        private readonly Matrix<float> _fbuf, _ebuf, _subm;
        private readonly Matrix<float>[] _outgoing, _padded, _input;
        private readonly Matrix<float>[][] _deltas;

        public ConvolutionalLayerBackprop(ConvolutionalLayer unit) {
            int fsize = unit.Weights[0][0].RowCount;
            _unit = unit;
            _outgoing = new Matrix<float>[unit.Prev.ChannelCount];
            for (int i = 0; i < _outgoing.Length; i++)
                _outgoing[i] = Matrix<float>.Build.Dense(unit.Prev.SideLength, unit.Prev.SideLength);
            _padded = new Matrix<float>[unit.Prev.ChannelCount];
            for (int i = 0; i < _padded.Length; i++)
                _padded[i] = Matrix<float>.Build.Dense(unit.Prev.SideLength + fsize - 1, unit.Prev.SideLength + fsize - 1);
            _input = new Matrix<float>[unit.Prev.ChannelCount];
            for (int i = 0; i < _input.Length; i++)
                _input[i] = Matrix<float>.Build.Dense(unit.Prev.SideLength + fsize - 1, unit.Prev.SideLength + fsize - 1);
            _ebuf = Matrix<float>.Build.Dense(unit.SideLength, unit.SideLength);
            _fbuf = Matrix<float>.Build.Dense(fsize, fsize);
            _subm = Matrix<float>.Build.Dense(fsize, fsize);
            _deltas = new Matrix<float>[unit.Prev.ChannelCount][];
            for (int i = 0; i < _deltas.Length; i++) {
                _deltas[i] = new Matrix<float>[unit.ChannelCount];
                for (int j = 0; j < unit.ChannelCount; j++)
                    _deltas[i][j] = Matrix<float>.Build.Dense(fsize, fsize);
            }
        }

        public Matrix<float>[] Visit(Matrix<float>[] incoming, BackpropParams par) {
            int stride = _unit.Stride;
            int fsize = _unit.Weights[0][0].RowCount;
            int offset = fsize / 2;
            // Clear next layer's error and cache input from previous forward pass.
            var inp = _unit.Prev.Output();
            for (int i = 0; i < _unit.Prev.ChannelCount; i++) {
                _padded[i].Clear();
                _input[i].SetSubMatrix(offset, _unit.Prev.SideLength, offset, _unit.Prev.SideLength, inp[i]);
            }
            for (int j = 0; j < _unit.ChannelCount; j++) {
                // Multiply incoming error term with derivative of this layer's activation function.
                _unit.Activation.Derivatives(_unit.Output()[j], _ebuf);
                incoming[j].PointwiseMultiply(_ebuf, incoming[j]);
                // Propagate error to next layer. Adjust weights and biases.
                for (int i = 0; i < _unit.Prev.ChannelCount; i++) {
                    _deltas[i][j].Multiply(par.Momentum, _deltas[i][j]); // Apply momentum.
                    for (int m = 0; m < incoming[j].RowCount; m++)
                        for (int n = 0; n < incoming[j].ColumnCount; n++) {
                            // Propagate error.
                            _unit.Weights[i][j].Multiply(incoming[j].At(m, n), _fbuf);
                            _subm.SetSubMatrix(0, m * stride, fsize, 0, n * stride, fsize, _padded[i]);
                            _subm.Add(_fbuf, _subm);
                            _padded[i].SetSubMatrix(m * stride, fsize, n * stride, fsize, _subm);
                            // Calculate deltas.
                            _subm.SetSubMatrix(0, m * stride, fsize, 0, n * stride, fsize, _input[i]);
                            _subm.Multiply(incoming[j].At(m, n) * par.LearningRate, _fbuf);
                            _deltas[i][j].Add(_fbuf, _deltas[i][j]);
                        }
                    _unit.Weights[i][j].Multiply(1f - par.Decay, _unit.Weights[i][j]); // Weight decay.
                    _unit.Weights[i][j].Add(_deltas[i][j], _unit.Weights[i][j]); // Adjust weights.
                }
                _unit.Biases.At(j, _unit.Biases.At(j) + incoming[j].RowSums().Sum() * par.LearningRate); // Adjust biases.
            }
            for (int i = 0; i < _unit.Prev.ChannelCount; i++)
                _outgoing[i].SetSubMatrix(0, offset, _unit.Prev.SideLength, 0, offset, _unit.Prev.SideLength, _padded[i]);
            return _outgoing;
        }
	}
}
