using UnityEngine;
using System.Collections;
using QAI.Utility;
using QAI.Training;
using QAI.Agent;

public class SlotBenchmark : QTester {
	private bool _testStarted;
	private int _crashes;
	private bool _isCrashing;
	private float _maxDist;

	private float _timeStart;

	#region implemented abstract members of QTester
	public override void Init (){
		_crashes = 0;
		_isCrashing = false;
		_testStarted = false;
		_maxDist = 0;
	}

	public override bool SetupNextTest (QAgent agent) {
		if (_testStarted)
			return false;
		_timeStart = Time.time;
		_testStarted = true;
		return true;
	}

	public override void OnActionTaken (QAgent agent, QAction action, QState state) {
		var car = (SlotCar)agent;
		if (!car.OnTrack && !_isCrashing) {
			_isCrashing = true;
			_crashes++;
		} else if (car.OnTrack && _isCrashing) {
			_isCrashing = false;
		}
		_maxDist = car.DistanceTravelled - car.StartPosition;

	}

	public override void OnTestComplete (double reward) {

	}

	public override void OnRunComplete () {
		Debug.Log(string.Format("Time: {0:F} Crashes: {1:D} Distance: {2:F}", Time.time - _timeStart, _crashes, _maxDist));
		BenchmarkSave.WriteSlotResult(Time.time - _timeStart, _crashes, _maxDist);
	}
	#endregion
}
