using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using QNetwork.Training;

namespace QNetwork.CNN {
	public class MeanPoolLayer : SpatialLayer {
        public int PoolSize { get { return size; } }

        private int size;

        public MeanPoolLayer(int size, SpatialLayer prev) : base((prev.SideLength - size) / size + 1, prev.ChannelCount) {
            values = new Matrix<float>[ChannelCount];
            for (int i = 0; i < values.Length; i++)
                values[i] = Matrix<float>.Build.Dense(SideLength, SideLength);
            Prev = prev;
            prev.Next = this;
            this.size = size;
        }

        public override Matrix<float>[] Compute(Matrix<float>[] input) {
            for (int i = 0; i < values.Length; i++)
                for (int m = 0; m < values[i].RowCount; m += size)
                    for (int n = 0; n < values[i].ColumnCount; n += size)
                        values[i].At(m, n, input[i].SubMatrix(m, size, n, size).EnumerateRows().Select(r => r.Average()).Average());
            return values;
        }

        public override T Accept<T>(Trainer<T> t, T state) {
            return t.Visit(this, state);
        }
	}
}
