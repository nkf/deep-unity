using UnityEngine;
using System.Collections;

public class QOption {
	public bool Discretize;
	public int TrainingInterval = 20;
	public int TrainingCycle = 10;
	public int BatchSize = 2000;
	public int MaxPoolSize = 2000;
	public float Discount = 0.95f;
	public float EpsilonStart = 0.5f;
	public float EpsilonEnd = 0.1f;

	public QOption() : base() {}
}
