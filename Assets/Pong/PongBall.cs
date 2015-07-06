using System.Linq;
using UnityEngine;

namespace Pong {
    class PongBall : MonoBehaviour {
        private PongGame _game;
        private Vector3 _velocity;
        private float _speed;
        private PongController Player1;
        private PongController Player2;

        private static readonly float[] pos = {0.2f, 0.6f, -0.6f, -0.2f};

        void Awake() {
            _game = FindObjectOfType<PongGame>();
            _speed = 5f;
            _velocity = new Vector3(-1, Random.Range(-1f, 1f)).normalized * _speed;
            //_velocity = new Vector3(-1, -.6f).normalized * _speed;
            //_velocity = new Vector3(-1, pos.Random().First()).normalized * _speed;
            transform.position += new Vector3(9, Random.Range(-6f,6f));
            var pcs = FindObjectsOfType<PongController>();
            Player1 = pcs.First(pc => pc.Side == Player.Player1);
            Player2 = pcs.First(pc => pc.Side == Player.Player2);
        }
        void FixedUpdate () {
            var ball = PongGame.BoundsFromTransform(transform);
            var p = transform.position + _velocity * Time.fixedDeltaTime;
            p.x -= ball.size.x/2;
            p.y -= ball.size.y/2;

            //Check vs game borders
            if        (ball.max.y > _game.Border.max.y) { //Top
                _velocity = Vector3.Reflect(_velocity, Vector3.down);
                SetTransformY(_game.Border.max.y - ball.size.y/2);
            } else if (ball.min.y < _game.Border.min.y) { //Bot
                _velocity = Vector3.Reflect(_velocity, Vector3.up);
                SetTransformY(_game.Border.min.y + ball.size.y/2);
            }  
        
            var p1 = PongGame.BoundsFromTransform(Player1.transform);
            var p2 = PongGame.BoundsFromTransform(Player2.transform);
            if (ball.Intersects(p1)) { //Player 1 controller
                _velocity = Vector3.Reflect(_velocity, Vector3.right).normalized * ++_speed;
                SetTransformX(p1.max.x + ball.size.x / 2);
                Player1.Hits++;
            } else if(ball.min.x < _game.Border.min.x) { //Player 1 Goal
                _game.Score(Player.Player1);
                //Reset(1);
            }
            
            if (ball.Intersects(p2)) { //Player 2 controller
                _velocity = Vector3.Reflect(_velocity, Vector3.left).normalized * ++_speed;
                SetTransformX(p2.min.x - ball.size.x / 2);
                Player2.Hits++;
            } else if(ball.max.x > _game.Border.max.x) { //Player 2 Goal
                _game.Score(Player.Player2);
                //Reset(-1);
            }

            //Update position
            transform.position += _velocity * Time.fixedDeltaTime;
            PongGame.DebugDrawBounds(PongGame.BoundsFromTransform(transform), Color.red);
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
            Reset(new Vector2(direction, Random.Range(-1f, 1f)));
        }

        public void Reset(Vector2 direction) {
            Player1.Hits = 0;
            Player2.Hits = 0;
            transform.position = Vector3.zero;
            _speed = 5f;
            _velocity = direction.normalized * _speed;
        }
        //Returns the winner if there is one
        public Player? IsTerminal() {
            var ball = PongGame.BoundsFromTransform(transform);
            if (ball.min.x < _game.Border.min.x) return Player.Player2;
            if (ball.max.x > _game.Border.max.x) return Player.Player1;
            return null;
        }

        public Vector3 Velocity {
            get { return _velocity; }
        }
    }
}
