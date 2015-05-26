using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.CNN {
	public class MeanPoolLayer : SpatialLayer {
        public int PoolSize { get; set; }

        public MeanPoolLayer(int size, SpatialLayer prev) : base((prev.SideLength - size) / size + 1, prev.ChannelCount) {
            _values = new Matrix<float>[ChannelCount];
            for (int i = 0; i < _values.Length; i++)
                _values[i] = Matrix<float>.Build.Dense(SideLength, SideLength);
            Prev = prev;
            PoolSize = size;
        }

        public override Matrix<float>[] Compute(Matrix<float>[] input) {
            for (int i = 0; i < _values.Length; i++)
                for (int m = 0; m < _values[i].RowCount; m += PoolSize)
                    for (int n = 0; n < _values[i].ColumnCount; n += PoolSize)
                        _values[i].At(m, n, input[i].SubMatrix(m, PoolSize, n, PoolSize).EnumerateRows().Select(r => r.Average()).Average());
            return _values;
        }

        public override T Accept<T>(Trainer<T> t, T state) {
            return t.Visit(this, state);
        }
	}
}
