using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.Experimental {
    public struct VectorPair {
        public Vector<float> left, right;
    }

	public class TreeLayer : TransformationLayer<VectorPair, Vector<float>> {
        private Vector<float> _values;
        public int LeftSize { get; private set; }
        public int RightSize { get; private set; }

        public TreeLayer(int leftsize, int rightsize) {
            _values = Vector<float>.Build.Dense(leftsize + rightsize);
            LeftSize = leftsize;
            RightSize = rightsize;
        }

        public override int Size() {
            return _values.Count;
        }

        public override Vector<float> Compute(VectorPair input) {
            _values.SetSubVector(0, LeftSize, input.left);
            _values.SetSubVector(LeftSize, RightSize, input.right);
            return _values;
        }

        public override Vector<float> Output() {
            return _values;
        }
	}
}
