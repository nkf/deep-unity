﻿using System;

[Serializable]
public class SARS {
    public readonly QState State;
	public readonly QState NextState;
	public readonly QAction Action;
	public readonly float Reward;
    public float Priority { get; set; }
	
	public SARS(QState s, QAction a, float r, QState s0) {
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
