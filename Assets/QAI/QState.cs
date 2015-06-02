using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

[Serializable]
public struct QState {
    public readonly Matrix<float> Features;
    public readonly float Reward;
    public readonly bool IsTerminal;
    public QState(Matrix<float> features, float reward, bool isTerminal) : this() {
        Features = features;
        Reward = reward;
        IsTerminal = isTerminal;
    }

    public bool Equals(QState other) {
        return Reward.Equals(other.Reward) 
            && IsTerminal.Equals(other.IsTerminal)
            && Features.Equals(other.Features);
    }

    public override bool Equals(object obj) {
        if(ReferenceEquals(null, obj)) return false;
        return obj is QState && Equals((QState)obj);
    }

    public override int GetHashCode() {
        unchecked {
            var hashCode = (Features != null ? Features.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ Reward.GetHashCode();
            hashCode = (hashCode * 397) ^ IsTerminal.GetHashCode();
            return hashCode;
        }
    }

    private static int Hash(double[] a) {
        return a.Aggregate(a.Length, (current, t) => unchecked( current*31 + t.GetHashCode() ));
    }
}
