using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.CNN {
	public class FlattenLayerBackprop : Backprop<Vector<float>, Matrix<float>[]> {
        private readonly FlattenLayer _unit;
        private readonly Matrix<float>[] _outgoing;

        public FlattenLayerBackprop(FlattenLayer unit) {
            _unit = unit;
            _outgoing = new Matrix<float>[unit.Z];
            for (int i = 0; i < _outgoing.Length; i++)
                _outgoing[i] = Matrix<float>.Build.Dense(unit.X, unit.Y);
        }

        public Matrix<float>[] Visit(Vector<float> incoming, BackpropParams par) {
            // Unflatten error.
            for (int j = 0; j < _unit.Z; j++)
                for (int m = 0; m < _unit.X; m++)
                    incoming.CopySubVectorTo(_outgoing[j].Row(m), j * _unit.X * _unit.Y + m * _unit.Y, 0, _unit.Y);
            return _outgoing;
        }
	}
}
