using UnityEngine;
using System.Collections;

public class QOption {
	public bool Discretize;
	public int TrainingInterval = 20;
	public int TrainingCycle = 10;
	public int BatchSize = 2000;
	public int MaxPoolSize = 2000;

	public QOption() : base() {}
}
