using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.MLP {
	public class DenseLayerBackprop : Backprop<Vector<float>, Vector<float>> {
        private readonly DenseLayer _unit;
        private readonly Vector<float> _outgoing, _vbuf;
        private readonly Matrix<float> _deltas, _mbuf;

        public DenseLayerBackprop(DenseLayer unit) {
            _unit = unit;
            _outgoing = Vector<float>.Build.Dense(unit.Prev.Size());
            _vbuf = Vector<float>.Build.Dense(unit.Size());
            _deltas = Matrix<float>.Build.Dense(unit.Size(), unit.Prev.Size());
            _mbuf = Matrix<float>.Build.Dense(unit.Size(), unit.Prev.Size());
        }

        public Vector<float> Visit(Vector<float> incoming, BackpropParams par) {
            // Multiply incoming error term with derivative of this layer's activation function.
            _unit.Activation.Derivatives(_unit.Output(), _vbuf);
            incoming.PointwiseMultiply(_vbuf, incoming);
            // Calculate outgoing error term (first factor of next layer's error) based on weights and errors in this layer.
            _unit.Weights.TransposeThisAndMultiply(incoming, _outgoing);
            // Calculate delta weights (applying momentum).
            incoming.OuterProduct(_unit.Prev.Output(), _mbuf);
            _mbuf.Multiply(par.LearningRate, _mbuf);
            _deltas.Multiply(par.Momentum, _deltas);
            _deltas.Add(_mbuf, _deltas);
            // Adjust weights and biases.
            _unit.Weights.Add(_deltas, _unit.Weights);
            incoming.Multiply(par.LearningRate, _vbuf);
            _unit.Biases.Add(_vbuf, _unit.Biases);
            return _outgoing;
        }
	}
}
