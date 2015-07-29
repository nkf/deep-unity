using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QAI;
using QAI.Agent;
using QAI.Utility;
using MathNet.Numerics.LinearAlgebra;
using QAI.Learning;
using QNetwork.CNN;
using UnityEngine;

namespace Pong {
    class PongController : MonoBehaviour, QAgent {
        public Vector3 StartPosistion { get; private set; }

        PongGame _game;
        PongBall _ball;

        QGrid _grid;
        Vector<float> _vect;

        public int Hits { get; set; } //Set by pongball

        //Set in editor
        public Player Side = 0;
        readonly KeyCode[][] _keys = {
            new[] {KeyCode.W, KeyCode.S},
            new[] {KeyCode.UpArrow, KeyCode.DownArrow}
        };

        void Start() {
            StartCoroutine(Movement());
            StartPosistion = transform.position;
            _game = FindObjectOfType<PongGame>();
            _ball = FindObjectOfType<PongBall>();
            if (Side == Player.Player1) {
                _grid = new QGrid(16, transform,
                    new GridSettings {Offset = new Vector3(9.8f, 0, 0), ResolutionX = 1.28f, ResolutionY = 1.28f});
                _vect = Vector<float>.Build.Dense(new[] { 1f });
                QAIManager.InitAgent(this, new QOption {
                    LearningRate = 0.005f,
                    NetworkArgs = new []{ new CNNArgs { FilterSize = 4, FilterCount = 1, PoolLayerSize = 2, Stride = 2 }},
                    MaxPoolSize = 2000,
                    BatchSize = 2000,
                    EpsilonStart = 0.5f,
                    EpsilonEnd = 0.1f,
                    Discount = 0.95f,
                    TrainingInterval = 20,
                    TrainingCycle = 10,
                });
            }
        }

        void FixedUpdate() {
            if (Side == Player.Player1) {
                _grid.DebugDraw(v => Color.Lerp(Color.black, Color.white, v/250f));
                QAIManager.GetAction(GetState())();
            }
        }

        IEnumerator Movement() {
            var keys = _keys[(int)Side];
            while (true) {
                Action action = Idle;
                if (Input.GetKey(keys[0])) action = MoveUp;
                if (Input.GetKey(keys[1])) action = MoveDown;
                if(Side == Player.Player1) QAIManager.Imitate(this, action);
				if(Side == Player.Player2) action();
                yield return new WaitForFixedUpdate();
            }
        }

        const float SpeedModifer = 10;
        void Move(Vector3 direction) {
            //Move controller
            transform.position += direction * (Time.fixedDeltaTime * SpeedModifer);
        
            //Stay within game border
            var pos = transform.position;
            var h = transform.localScale.y/2f;
            pos.y = Mathf.Min(pos.y+h, _game.Border.max.y) - h;
            pos.y = Mathf.Max(pos.y-h, _game.Border.min.y) + h;
            transform.position = pos;
        }

        [QBehavior]
        public void MoveUp() {
            Move(Vector3.up);
        }
        [QBehavior]
        public void MoveDown() {
            Move(Vector3.down);
        }
        [QBehavior]
        public void Idle() {}

        private List<Coordinates?> _prevPositions = new List<Coordinates?>(); 
        public QState GetState() {
            var winner = _ball.IsTerminal();
            float reward;
            bool terminal;
            //var terminal = winner.HasValue;
            //var reward = terminal ? (winner.Value == Side ? 1 : 0) : 0;
            var b = PongGame.BoundsFromTransform(_ball.transform);
            var controller = PongGame.BoundsFromTransform(transform);
            controller.size += new Vector3(0.2f,0,0);
			PongGame.DebugDrawBounds(controller, Color.blue);
            if (b.Intersects(controller)) {
                reward = 1;
                terminal = true;
                if(PongBenchmark.Running) terminal = winner.HasValue;
            } else {
                terminal = winner.HasValue;
                reward = terminal ? (winner.Value == Side ? 1 : 0) : 0;
            }
        

            //Calculate distance to top and bottom
            var topDist = _game.Border.max.y - controller.max.y;
            var botDist = controller.min.y - _game.Border.min.y;
        
            var bp = _ball.transform.position;
            var rbp = bp - transform.position;

            var gridmid = _grid.GridSize/2f;
            var top = bp.y >= _grid.Center.y;
            var gbp = _grid.Locate(bp);
            var bpy = gbp.HasValue ? gbp.Value.y : -1;
            _grid.Populate((bo, c) => {
                //var x = bo.center.x;
                //var v = bo.Contains(new Vector3(x, _game.Border.min.y)) || bo.Contains(new Vector3(x, _game.Border.max.y)) ? 0.3f : 0; // walls
                var ham = gbp.HasValue ? HammingDistance(gbp.Value, c) : int.MaxValue; // Hamming distance
                var v = ham <= 0 ? 1f : ham <= 1 ? 0.5f : 0; // ball
				//v = bo.Contains(bp + _ball.Velocity.normalized * 2) ? 150f : v;
				//v = bo.Intersects(controller) ? 100 : v;
                return v;
            });
            var state = _grid.Matrix;
            return new QState(new[] { state }, _vect.Clone(), reward, terminal);
        }

        public AIID AI_ID() {
            return new AIID("PongAI");
        }

        private void SetGridValues(QGrid grid, IEnumerable<Coordinates?> coords, float value) {
            foreach (var coord in coords.Where(c => c.HasValue).Select(c => c.Value)) {
                grid[coord] = value;
            }
        }

        private int HammingDistance(Coordinates a, Coordinates b) {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }
    }

    internal enum Player {
        Player1 = 0, Player2 = 1
    }
}
