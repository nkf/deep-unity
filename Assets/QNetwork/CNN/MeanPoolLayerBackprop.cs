using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.CNN {
    public class MeanPoolLayerBackprop : Backprop<Matrix<float>[], Matrix<float>[]> {
        private readonly MeanPoolLayer _unit;
        private readonly Matrix<float>[] _outgoing;
        private readonly Matrix<float> _ones;

        public MeanPoolLayerBackprop(MeanPoolLayer unit) {
            _unit = unit;
            _outgoing = new Matrix<float>[unit.Prev.ChannelCount];
            for (int i = 0; i < _outgoing.Length; i++)
                _outgoing[i] = Matrix<float>.Build.Dense(unit.Prev.SideLength, unit.Prev.SideLength);
            _ones = Matrix<float>.Build.Dense(unit.PoolSize, unit.PoolSize, 1f);
        }

        public Matrix<float>[] Visit(Matrix<float>[] incoming, BackpropParams par) {
            // Upsample the error by first taking the Kronecker product of a matrix of 1's the same size as the pooling region.
            // Then divide by the total size of the pooling region, thus distributing the error by mean.
            for (int j = 0; j < _unit.ChannelCount; j++) {
                incoming[j].KroneckerProduct(_ones, _outgoing[j]);
                _outgoing[j].Divide(_unit.PoolSize * _unit.PoolSize, _outgoing[j]);
            }
            return _outgoing;
        }
    }
}
