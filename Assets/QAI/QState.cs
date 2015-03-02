using System;
using System.Linq;

public struct QState {
    public readonly int[] State;
    public readonly double Reward;
    public readonly bool IsTerminal;
    public QState(int[] state, double reward, bool isTerminal) : this() {
        State = state;
        Reward = reward;
        IsTerminal = isTerminal;
    }

    public override int GetHashCode() {
        int hash = 23;
        hash = hash*31 + IntArrayHash(State);
        hash = hash*31 + Reward.GetHashCode();
        hash = hash*31 + IsTerminal.GetHashCode();
        return hash;
    }

    public override bool Equals(object obj) {
        if (!(obj is QState)) return false;
        var that = (QState) obj;
        var r = true;
        r &= State.SequenceEqual(that.State);
        r &= Reward.Equals(that.Reward);
        r &= IsTerminal.Equals(that.IsTerminal);
        return r;
    }

    private int IntArrayHash(int[] array) {
        return array.Aggregate(array.Length, (current, t) => unchecked(current*314159 + t));
    }
}
