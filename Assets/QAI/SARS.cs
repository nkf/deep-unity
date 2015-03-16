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
}
