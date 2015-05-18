using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using QNetwork.Training;

namespace QNetwork.CNN {
	public class SpatialLayer : Layer, Unit<Matrix<float>[]> {
        private Vector<float> buffer;
        protected Matrix<float>[] values;
        public int SideLength { get; set; }
        public int ChannelCount { get; set; }

        public SpatialLayer(int dimension, int channels) {
            SideLength = dimension;
            ChannelCount = channels;
            buffer = Vector<float>.Build.Dense(Size());
        }

        public override int Size() {
            return SideLength * SideLength * ChannelCount;
        }

        public override Vector<float> Compute(Vector<float> input) {
            // It is pointless to input flat data to a spatial interpretation.
            throw new NotSupportedException();
        }

        public virtual Matrix<float>[] Compute(Matrix<float>[] input) {
            return values = input;
        }

        public override Vector<float> Output() {
            // Flatten spatial data by copying to the buffer.
            int i = 0;
            for (int c = 0; c < values.Length; c++)
                for (int m = 0; m < values[c].RowCount; m++)
                    for (int n = 0; n < values[c].ColumnCount; n++)
                        buffer[i++] = values[c].At(m, n);
            return buffer;
        }

        Matrix<float>[] Unit<Matrix<float>[]>.Output() {
            return values;
        }

        public Matrix<float>[] Output2D() {
            return values;
        }

        public override T Accept<T>(Trainer<T> t, T state) {
            return t.Visit(this, state);
        }
	}
}
