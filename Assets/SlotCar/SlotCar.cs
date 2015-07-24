using UnityEngine;
using System.Collections;
using UnityEditor;

public class SlotCar : MonoBehaviour {
	public BezierCurve Track;
	public AnimationCurve Acc;
	public float Speed;
	public bool AutoDrive;
	public int LapNumber;
	public GameObject CtrlPoint1;
	public GameObject CtrlPoint2;
	public GameObject Center;

	protected float velocity;
	protected float force;
	protected float forceSensetivity = 5.5f;
	protected float position = 1;
	protected float speederPosition = 0;
	protected float distanceTravelled = 0;

	protected int prevLap = 0;
	protected float lapTime;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate() {
		speederPosition = Mathf.Clamp01(speederPosition + (Input.GetKey(KeyCode.Space) || AutoDrive ? 1f : -1.5f) * Time.fixedDeltaTime);
		velocity = Acc.Evaluate(speederPosition) * Speed * Time.fixedDeltaTime;

		var current = transform.position;
		var mid = Track.GetPointAtDistance(distanceTravelled + velocity/2f);
		var next = Track.GetPointAtDistance(distanceTravelled += velocity);

		CtrlPoint2.transform.position = current;
		CtrlPoint1.transform.position = mid;
		transform.LookAt(next);
		transform.position = next;

		var dir = CircleCalculator.Direction.Straight;
		var c = CircleCalculator.CalculateCircleCenter(current, mid, next, out dir);
		if(c.HasValue)
			Center.transform.position = c.Value;

		var curvature = CircleCalculator.CalculateCurvature(current, mid, next);
		var dirValue = dir == CircleCalculator.Direction.Left ? -1
			: dir == CircleCalculator.Direction.Right ? 1 : 0;

		force = curvature * velocity * forceSensetivity * dirValue;

		Debug.DrawRay(mid, (mid - Center.transform.position).normalized * Mathf.Abs(force), Color.green);

		if(Mathf.Abs(force) > 1f) {
			Debug.Log (force);
			CarOffTrack();
		}

		LapNumber = (int)(distanceTravelled / Track.length);
		if(LapNumber != prevLap && LapNumber != 0) {
			prevLap = LapNumber;
			Debug.Log ("Time: " + lapTime);
			Debug.Log ("Score: " + Track.length / lapTime);
			lapTime = 0;
		} else {
			lapTime += Time.deltaTime;
		}

	}

	void CarOffTrack() {
		Application.LoadLevel(Application.loadedLevel);
	}
}
