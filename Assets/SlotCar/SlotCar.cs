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

	protected float Velocity;
	protected float Force;
	protected float ForceSensetivity = 5.5f;
	protected float Position = 1;
	protected float SpeederPosition = 0;
	protected float DistanceTravelled = 0;

	protected int PrevLap = 0;
	protected float LapTime;
    protected bool OnTrack;

    // Use this for initialization
	void Start () {
	    OnTrack = true;
	}
	
	// Update is called once per frame
	void FixedUpdate() {
	    if (OnTrack) {
	        UpdatePosistion();
	    }
	    LapNumber = (int)(DistanceTravelled / Track.length);
		if(LapNumber != PrevLap && LapNumber != 0) {
			PrevLap = LapNumber;
			Debug.Log ("Time: " + LapTime);
			Debug.Log ("Score: " + Track.length / LapTime);
			LapTime = 0;
		} else {
			LapTime += Time.deltaTime;
		}
	}

    void UpdatePosistion() {
        SpeederPosition = Mathf.Clamp01(SpeederPosition + (Input.GetKey(KeyCode.Space) || AutoDrive ? 1f : -1.5f) * Time.fixedDeltaTime);
        Velocity = Acc.Evaluate(SpeederPosition) * Speed * Time.fixedDeltaTime;

        var current = transform.position;
        var mid = Track.GetPointAtDistance(DistanceTravelled + Velocity / 2f);
        var next = Track.GetPointAtDistance(DistanceTravelled += Velocity);

        transform.LookAt(next);
        transform.position = next;

        var dir = CircleCalculator.Direction.Straight;
        var c = CircleCalculator.CalculateCircleCenter(current, mid, next, out dir);
        if (c.HasValue)
            Center.transform.position = c.Value;

        var curvature = CircleCalculator.CalculateCurvature(current, mid, next);
        var dirValue = dir == CircleCalculator.Direction.Left ? -1
            : dir == CircleCalculator.Direction.Right ? 1 : 0;

        Force = curvature * Velocity * ForceSensetivity * dirValue;

        Debug.DrawRay(mid, (mid - Center.transform.position).normalized * Mathf.Abs(Force), Color.green);

        if (Mathf.Abs(Force) > 1f) {
            CarOffTrack();
        }
    }

	void CarOffTrack() {
	    OnTrack = false;
	    StartCoroutine(DerailAnimation());
	}

    IEnumerator DerailAnimation() {
        var position = transform.position;
        var body = GetComponent<Rigidbody2D>();
        for (int i = 0; i < 10; i++) {
            body.AddRelativeForce(Vector2.right * Velocity * 1000);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(2);
        transform.position = position;
        Velocity = 0;
        SpeederPosition = 0;
        Force = 0;
        OnTrack = true;
    }
}
