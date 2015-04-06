using UnityEngine;
using System.Collections;

public class SimplePongAI : MonoBehaviour {

    private PongController _controller;
    private PongBall _ball;
	void Start () {
	    _controller = GetComponent<PongController>();
	    _ball = FindObjectOfType<PongBall>();
	}
	
	void Update () {
	    var bPos = _ball.transform.position;
	    var cPos = transform.position;
        if(bPos.y > cPos.y) _controller.MoveUp();
        if(bPos.y < cPos.y) _controller.MoveDown();
	}
}
