using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.MLP {
	public class InputLayer : Layer<Vector<float>> {
        private readonly int _size;
        private Vector<float> _buffer;

        public InputLayer(int size) {
            _size = size;
        }

        public override int Size() {
            return _size;
        }

        public override Vector<float> Compute(Vector<float> input) {
            return _buffer = input;
        }

        public override Vector<float> Output() {
            return _buffer;
        }
	}
}
