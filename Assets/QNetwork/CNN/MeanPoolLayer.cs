using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.CNN {
	public class MeanPoolLayer : SpatialLayer {
        public int PoolSize { get; private set; }
        private int _area;

        public MeanPoolLayer(int size, SpatialLayer prev) : base(prev.SideLength / size, prev.ChannelCount) {
            _values = new Matrix<float>[ChannelCount];
            for (int i = 0; i < _values.Length; i++)
                _values[i] = Matrix<float>.Build.Dense(SideLength, SideLength);
            Prev = prev;
            PoolSize = size;
            _area = size * size;
        }

        public override Matrix<float>[] Compute(Matrix<float>[] input) {
            for (int i = 0; i < _values.Length; i++)
                for (int m = 0; m < _values[i].RowCount; m++)
                    for (int n = 0; n < _values[i].ColumnCount; n++) {
                        float sum = 0;
                        for (int x = 0; x < PoolSize; x++)
                            for (int y = 0; y < PoolSize; y++)
                                sum += input[i].At(m * PoolSize + x, n * PoolSize + y);
                        _values[i].At(m, n, sum / _area);
                    }
            return _values;
        }
	}
}
