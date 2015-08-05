using System;
using UnityEngine;
using System.Collections;
using QAI;
using QAI.Agent;
using QAI.Utility;
using QAI.Learning;
using UnityEditor;
using UnityEngine.Rendering;
using MathNet.Numerics.LinearAlgebra;
using QNetwork.CNN;


public class SlotCar : MonoBehaviour, QAgent {
	public bool AiControlled;
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

	public float Velocity;
	protected float Force;
	protected float ForceSensetivity = 0.12f;
	protected float Position = 1;
	protected float SpeederPosition = 0;
	private float _distanceTravelled = 0;
	public float DistanceTravelled { get { return _distanceTravelled; } }

	protected int PrevLap = 0;
	protected float LapTime;
    private bool _onTrack;
	public bool OnTrack { get { return _onTrack; } }

    private QGrid _grid;
	private Vector<float> _vector;
	private Bin _velocityBin;
	private Bin _forceBin;
	private float lastReward;
	private int standStillTicks = 0;


    // Use this for initialization
	void Start () {
	    GetComponentInChildren<SpriteRenderer>().shadowCastingMode = ShadowCastingMode.On;
	    _onTrack = true;
		_distanceTravelled = StartPosition;
		Track.GetPointAtDistance(_distanceTravelled);
        _grid = new QGrid(16, transform, new GridSettings { Offset = Vector3.up * 5.2f });
		_vector = Vector<float>.Build.Dense(10,0);
		_velocityBin = new Bin(0.01f, 0.25f, 0.5f, 75f);
		_forceBin = new Bin(0.01f, 0.25f, 0.5f, 75f);
        
		var options = new QOption { 
			Discretize = false,
			MaxPoolSize = 2000,
			BatchSize = 2000,
			EpsilonStart = 0.7f,
			Discount = 0.8f,
			TrainingInterval = 50,
			NetworkArgs = new []{ new CNNArgs { FilterSize = 4, FilterCount = 1, PoolLayerSize = 2, Stride = 2 } }
		};
		options.Discretize = false;

		if(AiControlled) {
			QAIManager.InitAgent(this, options);
			if(QAIManager.CurrentMode == QAIMode.Learning)
				Time.timeScale = 2.0f;
		}
	}
	
	// Update is called once per frame
	void FixedUpdate() {
	    if (_onTrack) {
			if(AiControlled) QAIManager.GetAction(GetState())();
	        UpdatePosistion();
	    }
	    LapNumber = (int)((_distanceTravelled - StartPosition) / Track.length);
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
        var mid = Track.GetPointAtDistance(_distanceTravelled + distUpdate / 2f);
        var next = Track.GetPointAtDistance(_distanceTravelled += distUpdate);

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

        Debug.DrawRay(mid, (mid - Center.transform.position).normalized * Mathf.Abs(Force), Color.green, 4);

        if (Mathf.Abs(Force) > 1f) {
            CarOffTrack(dirValue);
        }
    }

	public void Update() {
		_grid.DebugDraw(f => f > 0 ? Color.white : Color.black);
		_grid.Bounds.DebugDrawXY(Color.red);
	}

	void CarOffTrack(int dir) {
	    _onTrack = false;
		QAIManager.GetAction(GetState())();
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
        _onTrack = true;
    }

    void LookAt2D(Vector3 point) {
        Vector3 diff = (point - transform.position).normalized;
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
    }

    [QBehavior]
    private void FullSpeed() {
        AutoDrive = true;
    }
    [QBehavior]
    private void Brake() {
        AutoDrive = false;
    }

    public QState GetState() {
        _grid.SetAll(1f);
        for (int i = -2; i < 20; i++) {
            var point = Track.GetPointAtDistance(_distanceTravelled + i * 0.8f);
            var coordinates = _grid.Locate(point);
            if (coordinates.HasValue) {
                _grid[coordinates.Value] = 0f;
            }
        }

		_vector.SetSubVector(0, 5, _velocityBin.Get(Velocity/20f));
		_vector.SetSubVector(5,5, _forceBin.Get(Mathf.Abs(Force)));
//		_vector[0] = Velocity / 20f;
//		_vector[1] = Mathf.Abs(Force);

//		var reward = Mathf.Abs(DistanceTravelled - StartPosition) - lastReward > 0.001f ? 0.5f : 0f;
		var reward = Velocity / 20f;
		reward += Mathf.Abs(Force) * -0.2f;
//		reward = !OnTrack && Mathf.Abs(Force) > 1f ? 0f : reward;
//		reward +=  DistanceTravelled - StartPosition > 80 ? 80 / LapTime : 0;

		if (standStillTicks > 20)
			reward = 0;

		var terminal = 
			_distanceTravelled - StartPosition > Track.length 
			|| !_onTrack
			|| standStillTicks > 200;

		if (QAIManager.CurrentMode == QAIMode.Testing)
			terminal = _distanceTravelled - StartPosition > Track.length
				|| standStillTicks > 200;

        var state = new QState(
			new []{_grid.Matrix},
			_vector,
//			!terminal ? 0 : (DistanceTravelled - StartPosition) / (Track.length/2), 
			reward,
			terminal);

		if (Velocity < 1f)
			standStillTicks ++;
		else
			standStillTicks = 0;

//		var state = new QState(
//			new []{_grid.Matrix},
//			_vector.Clone(),
//			Mathf.Abs(DistanceTravelled - StartPosition) - lastReward > 0.001f ? Velocity / 20f : 0,
//			terminal
//		);

		lastReward = Mathf.Abs(_distanceTravelled - StartPosition);

//		Debug.Log (state.Reward);

		return state;
    }

    public AIID AI_ID() {
        return new AIID("SlotCar");
    }
}
