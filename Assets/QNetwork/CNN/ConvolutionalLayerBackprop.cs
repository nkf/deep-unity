using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.CNN {
	public class ConvolutionalLayerBackprop : Backprop<Matrix<float>[], Matrix<float>[]> {
        private readonly ConvolutionalLayer _unit;
        private readonly Matrix<float> _fbuf, _ebuf;
        private readonly Matrix<float>[] _outgoing;
        private readonly Matrix<float>[][] _deltas;

        public ConvolutionalLayerBackprop(ConvolutionalLayer unit) {
            _unit = unit;
            _outgoing = new Matrix<float>[unit.Prev.ChannelCount];
            for (int i = 0; i < _outgoing.Length; i++)
                _outgoing[i] = Matrix<float>.Build.Dense(unit.Prev.SideLength, unit.Prev.SideLength);
            _ebuf = Matrix<float>.Build.Dense(unit.SideLength, unit.SideLength);
            int fsize = unit.Weights[0][0].RowCount;
            _fbuf = Matrix<float>.Build.Dense(fsize, fsize);
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
            var input = _unit.Prev.Output();
            // Clear next layer's error.
            for (int i = 0; i < _unit.Prev.ChannelCount; i++)
                _outgoing[i].Clear();
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
                            var subm = _outgoing[i].SubMatrix(m * stride, fsize, n * stride, fsize);
                            subm.Add(_fbuf, subm);
                            // Calculate deltas.
                            input[i].SubMatrix(m * stride, fsize, n * stride, fsize).Multiply(incoming[j].At(m, n) * par.LearningRate, _fbuf);
                            _deltas[i][j].Add(_fbuf, _deltas[i][j]);
                        }
                    _unit.Weights[i][j].Add(_deltas[i][j], _unit.Weights[i][j]); // Adjust weights.
                }
                _unit.Biases.At(j, _unit.Biases.At(j) + incoming[j].RowSums().Sum() * par.LearningRate); // Adjust biases.
            }
            return _outgoing;
        }
	}
}
