using UnityEngine;
using System.Collections;

public class Carret : MonoBehaviour {
	protected Rigidbody2D body;
	public Transform child;
	bool hest;

	// Use this for initialization
	void Start () {
		body = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		body.AddRelativeForce(Vector2.left * 3f, ForceMode2D.Force);
		if(Time.timeSinceLevelLoad >= 10 && !hest) {
			hest = true;
			transform.position = Vector2.zero;
		}
	}
}
