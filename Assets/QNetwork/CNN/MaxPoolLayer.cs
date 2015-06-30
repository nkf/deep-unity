using System;
using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.CNN {
	public class MaxPoolLayer : SpatialLayer {
        public int PoolSize { get; private set; }
        public Matrix<float>[] Distribution { get; private set; }

        public MaxPoolLayer(int size, SpatialLayer prev) : base(prev.SideLength / size, prev.ChannelCount) {
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
                        float amax = float.NegativeInfinity, max = 0;
                        int posx = 0, posy = 0;
                        for (int x = 0; x < PoolSize; x++)
                            for (int y = 0; y < PoolSize; y++) {
                                float tmp = input[i].At(m * PoolSize + x, n * PoolSize + y);
                                if (Math.Abs(tmp) > amax) {
                                    posx = x;
                                    posy = y;
                                    amax = Math.Abs(max = tmp);
                                }
                            }
                        Distribution[i].At(m * PoolSize + posx, n * PoolSize + posy, 1f);
                        _values[i].At(m, n, max);
                    }
            return _values;
        }
	}
}
