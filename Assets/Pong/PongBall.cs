using System.Linq;
using Encog.ML.Bayesian.Training.Estimator;
using UnityEngine;
using System.Collections;

class PongBall : MonoBehaviour {

    
    private PongGame _game;
    private Vector3 _velocity;
    private float _speed;
    private PongController Player1;
    private PongController Player2;
    void Awake() {
        _game = FindObjectOfType<PongGame>();
        _speed = 5f;
        _velocity = new Vector3(-1, Random.Range(-1f,1)).normalized * _speed;
        //_velocity = new Vector3(-1, -1).normalized * _speed;
        var pcs = FindObjectsOfType<PongController>();
        Player1 = pcs.First(pc => pc.Side == Player.Player1);
        Player2 = pcs.First(pc => pc.Side == Player.Player2);
    }
	void FixedUpdate () {
	    var ball = PongGame.RectFromTransform(transform);
	    var p = transform.position + _velocity * Time.fixedDeltaTime;
	    p.x -= ball.width/2;
	    p.y -= ball.height/2;
        ball = PongGame.Encapsulate(ball, p);
        PongGame.DebugDrawRect(ball, Color.red);
        
        //Check vs game borders
        if        (ball.yMax > _game.Border.yMin) { //Top
            _velocity = Vector3.Reflect(_velocity, Vector3.down);
            SetTransformY(_game.Border.yMin - ball.height/2);
        } else if (ball.yMin < _game.Border.yMax) { //Bot
            _velocity = Vector3.Reflect(_velocity, Vector3.up);
            SetTransformY(_game.Border.yMax + ball.height/2);
        }  
        
	    var p1 = PongGame.RectFromTransform(Player1.transform);
	    var p2 = PongGame.RectFromTransform(Player2.transform);
	    if (ball.Overlaps(p1)) { //Player 1 controller
	        _velocity = Vector3.Reflect(_velocity, Vector3.right).normalized * ++_speed;
            SetTransformX(p1.xMax + ball.width / 2);
        } else if(ball.xMin < _game.Border.xMin) { //Player 1 Goal
            _game.Score(Player.Player1);
            Reset(1);
        }
            
        if (ball.Overlaps(p2)) { //Player 2 controller
            _velocity = Vector3.Reflect(_velocity, Vector3.left).normalized * ++_speed;
            SetTransformX(p2.xMin - ball.width / 2);
        } else if(ball.xMax > _game.Border.xMax) { //Player 2 Goal
            _game.Score(Player.Player2);
            Reset(-1);
        }

        //Update position
	    transform.position += _velocity * Time.fixedDeltaTime;
	}

    void SetTransformY(float y) {
        var pos = transform.position;
        pos.y = y;
        transform.position = pos;
    }

    void SetTransformX(float x) {
        var pos = transform.position;
        pos.x = x;
        transform.position = pos;
    }

    void Reset(int direction) {
        return;
        transform.position = Vector3.zero;
        _speed = 5f;
        _velocity = new Vector3(direction, Random.Range(-1f,1f)).normalized * _speed;
    }

    public Player? IsTerminal() {
        var ball = PongGame.RectFromTransform(transform);
        if (ball.xMin < _game.Border.xMin) return Player.Player1;
        if (ball.xMax > _game.Border.xMax) return Player.Player2;
        return null;
    }

    public Vector3 Velocity {
        get { return _velocity; }
    }
}
