using UnityEngine;
using System.Collections;

public class SlotCar : MonoBehaviour {
	public BezierCurve Track;
	public AnimationCurve Acc;
	protected float position = 1;
	protected float speederPosition = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
//		var trackPoint = track[position];
//		transform.position = trackPoint.transform.position;
//		transform.rotation = trackPoint.transform.rotation;
		speederPosition = Mathf.Clamp01(speederPosition + (Input.GetKey(KeyCode.Space) ? 0.01f : -0.01f));
		position += Acc.Evaluate(speederPosition) * 0.002f;
//		GetComponent<Rigidbody2D>().MovePosition(Track.GetPointAt(position%1f));
		transform.position = Track.GetPointAt(position%1f);
		Debug.Log (position%1);
	}
}
