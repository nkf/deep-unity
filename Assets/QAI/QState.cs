using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

[Serializable]
public struct QState {
    public readonly Matrix<float>[] Features;
    public readonly float Reward;
    public readonly bool IsTerminal;
    public QState(Matrix<float>[] features, float reward, bool isTerminal) : this() {
        Features = features;
        Reward = reward;
        IsTerminal = isTerminal;
    }

    public bool Equals(QState other) {
        return Features == other.Features || (Features != null && other.Features != null && Features.SequenceEqual(other.Features));
    }

    public override bool Equals(object obj) {
        if(ReferenceEquals(null, obj)) return false;
        return obj is QState && Equals((QState)obj);
    }

    public override int GetHashCode() {
        unchecked {
            return Features != null ? Hash(Features) : 0;
        }
    }

    private static int Hash<T>(T[] a) {
        return a.Aggregate(a.Length, (current, t) => unchecked( current*31 + t.GetHashCode() ));
    }
}
