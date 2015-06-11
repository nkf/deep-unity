using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.CNN {
	public class MaxPoolLayer : SpatialLayer {
        public int PoolSize { get; set; }
        public Matrix<float>[] Distribution { get; set; }

        public MaxPoolLayer(int size, SpatialLayer prev) : base((prev.SideLength - size) / size + 1, prev.ChannelCount) {
            _values = new Matrix<float>[ChannelCount];
            for (int i = 0; i < _values.Length; i++)
                _values[i] = Matrix<float>.Build.Dense(SideLength, SideLength);
            Prev = prev;
            PoolSize = size;
            Distribution = new Matrix<float>[prev.ChannelCount];
            for (int i = 0; i < Distribution.Length; i++)
                Distribution[i] = Matrix<float>.Build.Sparse(prev.SideLength, prev.SideLength);
        }

        public override Matrix<float>[] Compute(Matrix<float>[] input) {
            for (int i = 0; i < _values.Length; i++)
                for (int m = 0; m < _values[i].RowCount; m++)
                    for (int n = 0; n < _values[i].ColumnCount; n++) {
                        int x = -1, y = 0;
                        float max = float.NegativeInfinity;
                        input[i].SubMatrix(m * PoolSize, PoolSize, n * PoolSize, PoolSize).EnumerateRows().ForEach(r => {
                            float tmp = r.AbsoluteMaximum();
                            if (tmp > max) {
                                x++;
                                y = r.AbsoluteMaximumIndex();
                                max = tmp;
                            }
                        });
                        Distribution[i].At(m * PoolSize + x, n * PoolSize + y, 1f);
                        _values[i].At(m, n, max);
                    }
            return _values;
        }

        public override T Accept<T>(Trainer<T> t, T state) {
            return t.Visit(this, state);
        }
	}
}
