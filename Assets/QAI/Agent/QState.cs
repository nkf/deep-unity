using System;
using System.Linq;
using QNetwork.CNN;
using MathNet.Numerics.LinearAlgebra;

namespace QAI.Agent {
    [Serializable]
    public struct QState {
        private static readonly Vector<float> V1 = Vector<float>.Build.Dense(1, 1f);

        public readonly StatePair Features;
        public readonly float Reward;
        public readonly bool IsTerminal;
        public int GridSize { get { return Features.Spatial[0].RowCount; } }
        public int VectorSize { get { return Features.Linear.Count; } }
        public int Depth { get { return Features.Spatial.Length; } }
        public QState(Matrix<float>[] image, Vector<float> vector, float reward, bool isTerminal) : this() {
            Features = new StatePair(image, vector);
            Reward = reward;
            IsTerminal = isTerminal;
        }

        public QState(Matrix<float>[] image, float reward, bool isTerminal) : this() {
            Features = new StatePair(image, V1);
            Reward = reward;
            IsTerminal = isTerminal;
        }


        public bool Equals(QState other) {
            var img = Features.Spatial;
            var oimg = other.Features.Spatial;
            return (IsTerminal == other.IsTerminal 
				&& Reward == other.Reward 
				&& ((img == oimg)
				|| (img != null 
					    && oimg != null 
					    && img.SequenceEqual(oimg))))
                && Features.Linear.Equals(other.Features.Linear);
        }

        public override bool Equals(object obj) {
            if(ReferenceEquals(null, obj)) return false;
            return obj is QState && Equals((QState)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return Hash(Features.Spatial) * 31 + Features.Linear.GetHashCode();
            }
        }

        private static int Hash<T>(T[] a) {
            return a.Aggregate(a.Length, (current, t) => unchecked( current*31 + t.GetHashCode() ));
        }
    }
}
