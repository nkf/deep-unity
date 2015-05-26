using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.CNN {
	public class MaxPoolLayer : SpatialLayer {
        public int PoolSize { get { return _size; } }

        private readonly int _size;

        public MaxPoolLayer(int size, SpatialLayer prev) : base((prev.SideLength - size) / size + 1, prev.ChannelCount) {
            _values = new Matrix<float>[ChannelCount];
            for (int i = 0; i < _values.Length; i++)
                _values[i] = Matrix<float>.Build.Dense(SideLength, SideLength);
            Prev = prev;
            _size = size;
        }

        public override Matrix<float>[] Compute(Matrix<float>[] input) {
            for (int i = 0; i < _values.Length; i++)
                for (int m = 0; m < _values[i].RowCount; m += _size)
                    for (int n = 0; n < _values[i].ColumnCount; n += _size)
                        _values[i].At(m, n, input[i].SubMatrix(m, _size, n, _size).EnumerateRows().Select(r => r.AbsoluteMaximum()).Max());
            return _values;
        }

        public override T Accept<T>(Trainer<T> t, T state) {
            return t.Visit(this, state);
        }
	}
}
