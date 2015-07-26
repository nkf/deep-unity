using System;
using UnityEngine;
using System.Collections;
using QAI;
using QAI.Agent;
using QAI.Utility;
using UnityEditor;
using UnityEngine.Rendering;


public class SlotCar : MonoBehaviour, QAgent {
	public BezierCurve Track;
	public AnimationCurve Acc;
	public int StartPosition;
	public KeyCode SpeederKey;
	public float Speed;
	public bool AutoDrive;
	public int LapNumber;
	public GameObject CtrlPoint1;
	public GameObject CtrlPoint2;
	public GameObject Center;

	protected float Velocity;
	protected float Force;
	protected float ForceSensetivity = 0.12f;
	protected float Position = 1;
	protected float SpeederPosition = 0;
	protected float DistanceTravelled = 0;

	protected int PrevLap = 0;
	protected float LapTime;
    protected bool OnTrack;

    private Q2DGrid _grid;
    // Use this for initialization
	void Start () {
	    GetComponentInChildren<SpriteRenderer>().shadowCastingMode = ShadowCastingMode.On;
	    OnTrack = true;
		DistanceTravelled = StartPosition;
		Track.GetPointAtDistance(DistanceTravelled);
        _grid = new Q2DGrid(16, transform);
        
        QAIManager.InitAgent(this);
	}
	
	// Update is called once per frame
	void FixedUpdate() {
	    QAIManager.GetAction(GetState())();
	    if (OnTrack) {
	        UpdatePosistion();
	    }
	    LapNumber = (int)(DistanceTravelled - StartPosition / Track.length);
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
        SpeederPosition = Mathf.Clamp01(SpeederPosition + (Input.GetKey(SpeederKey) || AutoDrive ? 1f : -1.5f) * Time.fixedDeltaTime);
        if(Math.Abs(SpeederPosition) < 0.001f) return;
        Velocity = Acc.Evaluate(SpeederPosition)*Speed;

        var distUpdate = Velocity*Time.fixedDeltaTime;
        var current = transform.position;
        var mid = Track.GetPointAtDistance(DistanceTravelled + distUpdate / 2f);
        var next = Track.GetPointAtDistance(DistanceTravelled += distUpdate);

        LookAt2D(next);
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
            CarOffTrack(dirValue);
        }
    }

	void CarOffTrack(int dir) {
	    OnTrack = false;
	    StartCoroutine(DerailAnimation(dir));
	}

    IEnumerator DerailAnimation(int dir) {
        var collider = GetComponentInChildren<Collider2D>();
        var layer = collider.gameObject.layer;
        collider.gameObject.layer = LayerMask.NameToLayer("Default");
        var position = transform.position;
        var rotation = transform.rotation;
        var body = GetComponent<Rigidbody2D>();
        body.AddRelativeForce(Vector3.up * Velocity, ForceMode2D.Impulse);
        body.angularVelocity = Velocity * 200 * -dir;
        yield return new WaitForSeconds(2);
        body.velocity = Vector2.zero;
        body.angularVelocity = 0;
        transform.position = position;
        transform.rotation = rotation;
        collider.gameObject.layer = layer;
        Velocity = 0;
        SpeederPosition = 0;
        Force = 0;
        OnTrack = true;
    }

    void LookAt2D(Vector3 point) {
        Vector3 diff = (point - transform.position).normalized;
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
    }

    [QBehavior]
    private void CruiseControlOn() {
        AutoDrive = true;
    }
    [QBehavior]
    private void CruiseControlOff() {
        AutoDrive = false;
    }

    public QState GetState() {
        _grid.SetAll(0f);
        for (int i = -10; i < 10; i++) {
            var point = Track.GetPointAtDistance(DistanceTravelled + i*0.1f);
            var coordinates = _grid.Locate(point);
            if (coordinates.HasValue) {
                _grid[coordinates.Value] = 1f;
            }
        }
        return new QState(new []{_grid.Matrix}, 0, false);
    }

    public AIID AI_ID() {
        return new AIID("SlotCar");
    }
}
