using UnityEngine;
using System.Collections;

public struct SARS {
	public QState state;
	public QState nextState;
	public QAction action;
	public double reward;
	
	public SARS(QState s, QAction a, double r, QState s0) {
		state = s; action = a; reward = r; nextState = s0;
	}
}
