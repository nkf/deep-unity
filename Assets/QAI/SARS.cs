using UnityEngine;
using System.Collections;

public struct SARS {
    public QState State;
	public QState NextState;
	public QAction Action;
	public double Reward;
	
	public SARS(QState s, QAction a, double r, QState s0) {
		State = s; Action = a; Reward = r; NextState = s0;
	}

    public bool Equals(SARS other) {
        return State.Equals(other.State) && NextState.Equals(other.NextState) && Action.Equals(other.Action) && Reward.Equals(other.Reward);
    }

    public override int GetHashCode() {
        unchecked {
            int hashCode = State.GetHashCode();
            hashCode = (hashCode * 397) ^ NextState.GetHashCode();
            hashCode = (hashCode * 397) ^ Action.GetHashCode();
            hashCode = (hashCode * 397) ^ Reward.GetHashCode();
            return hashCode;
        }
    }
}
