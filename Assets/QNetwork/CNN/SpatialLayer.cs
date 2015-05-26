using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.CNN {
	public class SpatialLayer : Layer<Matrix<float>[]> {
        protected Matrix<float>[] _values;
        public int SideLength { get; set; }
        public int ChannelCount { get; set; }

        public SpatialLayer(int dimension, int channels) {
            SideLength = dimension;
            ChannelCount = channels;
        }

        public override int Size() {
            return SideLength * SideLength * ChannelCount;
        }

        public override Matrix<float>[] Compute(Matrix<float>[] input) {
            return _values = input;
        }

        public override Matrix<float>[] Output() {
            return _values;
        }

        public override T Accept<T>(Trainer<T> t, T state) {
            return t.Visit(this, state);
        }
    }
}
