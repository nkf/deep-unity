using UnityEngine;
using System.Collections;

public class SimplePongAI : MonoBehaviour {

    private PongController _controller;
    private PongBall _ball;
	void Start () {
	    _controller = GetComponent<PongController>();
	    _ball = FindObjectOfType<PongBall>();
	}
	
	void FixedUpdate () {
	    var b = _ball.transform.position.y;
	    var c = transform.position.y;
        if(b > c) _controller.MoveUp();
        if(b < c) _controller.MoveDown();
	}
}
