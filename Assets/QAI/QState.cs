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

    public override int GetHashCode() {
        throw new NotImplementedException();
        int hash = 23;
        hash = hash*31 + Reward.GetHashCode();
        hash = hash*31 + IsTerminal.GetHashCode();
        return hash;
    }

    public override bool Equals(object obj) {
        if (!(obj is QState)) return false;
        var that = (QState) obj;
        var r = true;
        r &= Features.SequenceEqual(that.Features);
        r &= Reward.Equals(that.Reward);
        r &= IsTerminal.Equals(that.IsTerminal);
        return r;
    }
}
