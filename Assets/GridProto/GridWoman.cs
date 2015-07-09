using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using QAI;
using QAI.Agent;
using QAI.Utility;
using UnityEngine;

namespace GridProto {
    public class GridWoman : MonoBehaviour, QAgent {
    
        private Q2DGrid _grid;
        private Bin _nvectorBin;
        private Vector<float> _linearState; 
        private void Start() {
            _grid = new Q2DGrid(13, transform, new GridSettings { NormalAxis = Axis.Y });
            _nvectorBin = new Bin(-0.75f,-0.25f,0.25f,0.75f);
            _linearState = Vector<float>.Build.Dense(2);
            QAIManager.InitAgent(this);
        }

        public bool IsAboveGround() {
            return Physics.Raycast(transform.position, Vector3.down);
        }

        [QBehavior]
        private void MoveUp() {
            transform.position += new Vector3(1, 0, 0);
        }

        [QBehavior]
        private void MoveDown() {
            transform.position += new Vector3(-1, 0, 0);
        }

        [QBehavior]
        private void MoveLeft() {
            transform.position += new Vector3(0, 0, 1);
        }

        [QBehavior]
        private void MoveRight() {
            transform.position += new Vector3(0, 0, -1);
        }

        private int[] PositionToState(Vector3 p) {
            return new[] {
                Mathf.RoundToInt(p.x),
                Mathf.RoundToInt(p.z),
            };
        }

        private Vector3 VectorToGoal(Vector3 p, Vector3 goal) {
            return goal - p;
        }

        private int[] PosAndGoal(int[] p, int[] goal) {
            var a = p;
            var b = goal;
            return new[] {a[0], a[1], a[2], b[0], b[1], b[2]};
        }

        private int[] PosToGoal(Vector3 p, Vector3 goal) {
            //var v = VectorToGoal(p, goal);
            var s = PositionToState(p);
            var g = PositionToState(goal);
            return new[] {s[0], s[2], g[0], g[2]};
        }


        private readonly Vector3 Min = new Vector3(-1, 0, -4);

        private int[] Offset(Vector3 p) {
            return PositionToState(p - Min);
        }

        public QState GetState() {
            var p = PositionToState(transform.position);
            var g = PositionToState(Goal.Position);
            
            _grid.Populate(bounds => {
                var ray = new Ray(new Vector3(bounds.center.x, 2, bounds.center.z), Vector3.down);
                
                RaycastHit hit;
                float r;
                if (Physics.Raycast(ray, out hit, 3.0f)) {
                    r = hit.collider.gameObject == Goal.Instance.gameObject ? 1f : 0.1f;
                }
                else r = 0f;
                Debug.DrawRay(ray.origin, ray.direction * 3.0f, r == 0f ? Color.red : r == 0.1f ? Color.gray : Color.yellow);

                return r;
            });
            var dead = !IsAboveGround();
            var goal = p.SequenceEqual(g);
            var v = VectorToGoal(transform.position, Goal.Position).normalized;
            _linearState.At(0, v.x);
            _linearState.At(1, v.z);
            return new QState(
                /*
            _grid.State
                 .Concat(new double[] {v.x, v.z})
                 .ToArray(),
            */
                new []{_grid.Matrix.Clone()},
                _linearState,
                goal ? 1 : 0,
                dead || goal
                );
        }

        public AIID AI_ID() {
            return new AIID("GridWomanAI");
        }


        public void FixedUpdate() {
            QAIManager.GetAction(GetState())();
            _grid.DebugDraw(value => value == 0 ? Color.red : value == 1 ? Color.gray : Color.yellow);
            Action currentAction = null;
            if (Key(KeyCode.UpArrow, KeyCode.W)) currentAction = MoveUp;
            if (Key(KeyCode.DownArrow, KeyCode.S)) currentAction = MoveDown;
            if (Key(KeyCode.RightArrow, KeyCode.D)) currentAction = MoveRight;
            if (Key(KeyCode.LeftArrow, KeyCode.A)) currentAction = MoveLeft;
            if (currentAction != null)
                QAIManager.Imitate(this, currentAction);
        }

        private bool Key(params KeyCode[] keys) {
            return keys.Any(kc => Input.GetKeyDown(kc));
        }
    }
}