using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.Experimental {
    public struct VectorPair {
        public Vector<float> left, right;
    }

	public class TreeLayer : TransformationLayer<VectorPair[], Vector<float>> {
        private Vector<float> _buffer;

        public TreeLayer(int leftsize, int rightsize) {
            _buffer = Vector<float>.Build.Dense(leftsize + rightsize);
        }

        public override int Size() {
            return _buffer.Count;
        }

        public override Vector<float> Compute(params VectorPair[] input) {
            //_buffer.SetSubVector(0, input.left.Count, input.left);
            //_buffer.SetSubVector(input.left.Count, input.right.Count, input.right);
            return _buffer;
        }

        public override Vector<float> Output() {
            return _buffer;
        }
	}
}
