using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.CNN {
	public class MaxPoolLayerBackprop : Backprop<Matrix<float>[], Matrix<float>[]> {
        private readonly MaxPoolLayer _unit;
        private readonly Matrix<float>[] _outgoing;
        private readonly Matrix<float> _ones;

        public MaxPoolLayerBackprop(MaxPoolLayer unit) {
            _unit = unit;
            _outgoing = new Matrix<float>[unit.Prev.ChannelCount];
            for (int i = 0; i < _outgoing.Length; i++)
                _outgoing[i] = Matrix<float>.Build.Dense(unit.Prev.SideLength, unit.Prev.SideLength);
            _ones = Matrix<float>.Build.Dense(unit.PoolSize, unit.PoolSize, 1f);
        }

        public Matrix<float>[] Visit(Matrix<float>[] incoming, BackpropParams par) {
            // Upsample the error by first taking the Kronecker product of a matrix of 1's the same size as the pooling region.
            // Then pointwise multiply by the pooling layer's distribution matrix.
            for (int j = 0; j < _unit.ChannelCount; j++) {
                incoming[j].KroneckerProduct(_ones, _outgoing[j]);
                _outgoing[j].PointwiseMultiply(_unit.Distribution[j], _outgoing[j]);
            }
            return _outgoing;
        }
	}
}
