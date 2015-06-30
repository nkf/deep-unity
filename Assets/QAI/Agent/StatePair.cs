using MathNet.Numerics.LinearAlgebra;

namespace QAI.Agent {
    public struct StatePair {
        public readonly Matrix<float>[] Spatial;
        public readonly Vector<float> Linear;
        public StatePair(Matrix<float>[] spatial, Vector<float> linear) : this() {
            Spatial = spatial;
            Linear = linear;
        }
    }
}
