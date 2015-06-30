using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.Experimental {
	public class TreeLayerBackprop : Backprop<Vector<float>, VectorPair> {
        private readonly TreeLayer _unit;
        private readonly VectorPair _outgoing;

        public TreeLayerBackprop(TreeLayer unit) {
            _unit = unit;
            _outgoing = new VectorPair {
                left = Vector<float>.Build.Dense(unit.LeftSize),
                right = Vector<float>.Build.Dense(unit.RightSize)
            };
        }
        
        public VectorPair Visit(Vector<float> incoming, BackpropParams par) {
            incoming.CopySubVectorTo(_outgoing.left, 0, 0, _unit.LeftSize);
            incoming.CopySubVectorTo(_outgoing.right, _unit.LeftSize, 0, _unit.RightSize);
            return _outgoing;
        }
	}
}
