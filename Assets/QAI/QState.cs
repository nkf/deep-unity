using System;
using System.Linq;

public struct QState {
    public readonly double[] Features;
    public readonly double Reward;
    public readonly bool IsTerminal;
    public QState(double[] features, double reward, bool isTerminal) : this() {
        Features = features;
        Reward = reward;
        IsTerminal = isTerminal;
    }

    public bool Equals(QState other) {
        return Features.SequenceEqual(other.Features) && Reward.Equals(other.Reward) && IsTerminal.Equals(other.IsTerminal);
    }

    public override bool Equals(object obj) {
        if(ReferenceEquals(null, obj)) return false;
        return obj is QState && Equals((QState)obj);
    }

    public override int GetHashCode() {
        unchecked {
            var hashCode = (Features != null ? Hash(Features) : 0);
            hashCode = (hashCode * 397) ^ Reward.GetHashCode();
            hashCode = (hashCode * 397) ^ IsTerminal.GetHashCode();
            return hashCode;
        }
    }

    private static int Hash(double[] a) {
        return a.Aggregate(a.Length, (current, t) => unchecked( current*31 + Hash(t) ));
    }

    private static int Hash(double d) {
        var bits = BitConverter.DoubleToInt64Bits(d);
        return unchecked( (int)(bits ^ (bits >> 32)) );
    }
}
