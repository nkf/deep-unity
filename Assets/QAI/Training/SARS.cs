using System;
using System.Collections.Generic;
using QAI.Agent;

namespace QAI.Training {
    [Serializable]
    public class SARS {
        public readonly QState State;
        public readonly QState NextState;
        public readonly QAction Action;
        public float Reward { get { return NextState.Reward; } }
        public float Priority { get; set; }
	
        public SARS(QState s, QAction a, QState s0) {
            State = s; Action = a; NextState = s0;
        }

        public bool Equals(SARS other) {
            return State.Equals(other.State) && NextState.Equals(other.NextState) && Action.Equals(other.Action);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = State.GetHashCode();
                hashCode = (hashCode * 397) ^ NextState.GetHashCode();
                hashCode = (hashCode * 397) ^ Action.GetHashCode();
                return hashCode;
            }
        }
    }

    public class SARSPrioritizer : IComparer<SARS> {
        public int Compare(SARS x, SARS y) {
            return (int)(x.Priority - y.Priority);
        }
    }
}
