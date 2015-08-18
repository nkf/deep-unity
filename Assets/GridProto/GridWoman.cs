using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using QAI;
using QAI.Agent;
using QAI.Utility;
using UnityEngine;

namespace GridProto {
    public class GridWoman : MonoBehaviour, QAgent {
        //Number of states that will be saved
        private const int MaxHistorySize = 30;
        //If less than this number of unique states is in history, we will declare it a cycle.
        private const int CycleSize = 6;

        private bool _testModel = false;
    
        private QGrid _grid;
        private Vector<float> _linearState;
        private LinkedList<QState> _history;
        private void Start() {
            _grid = new QGrid(13, transform, new GridSettings { NormalAxis = Axis.Y });
            _linearState = Vector<float>.Build.Dense(2);
            _history = new LinkedList<QState>();
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
                if (Physics.Raycast(ray, out hit, 3.0f)) {
                    return hit.collider.gameObject == Goal.Instance.gameObject ? 1f : 0.1f;
                }
                return 0f;
            });
            var dead = !IsAboveGround();
            var goal = p.SequenceEqual(g);
            var line = VectorToGoal(transform.position, Goal.Position);
            var v = line.normalized;
            _linearState.At(0, v.x);
            _linearState.At(1, v.z);

            var terminal = dead || goal || DetectCycle();
            var state = new QState(
                new []{ _grid.Matrix },
                _linearState.Clone(),
                goal ? 1 : 0,
                terminal
            );
            return state;
        }

        private void ArchiveState(QState state) {
            if(_history.Count >= MaxHistorySize) _history.RemoveLast();
            _history.AddFirst(state);
        }

        private bool DetectCycle() {
            if (_history.Count < MaxHistorySize) return false;
            return _history.Distinct().Count() <= CycleSize;
        }

        public AIID AI_ID() {
            return new AIID("GridWomanAI");
        }


        public void FixedUpdate() {
            if (_testModel) {
                Instantiate(Resources.Load<ModelTest>("ModelTest"));
                _testModel = false;
            }
            _grid.DebugDraw(value => value == 0 ? Color.red : value == 1 ? Color.gray : Color.yellow);
            if (QAIManager.CurrentMode != QAIMode.Imitating) {
                var state = GetState();
                QAIManager.GetAction(state)();
                ArchiveState(state);
            } else {
                Action currentAction = null;
                if (Key(KeyCode.UpArrow,    KeyCode.W)) currentAction = MoveUp;
                if (Key(KeyCode.DownArrow,  KeyCode.S)) currentAction = MoveDown;
                if (Key(KeyCode.RightArrow, KeyCode.D)) currentAction = MoveRight;
                if (Key(KeyCode.LeftArrow,  KeyCode.A)) currentAction = MoveLeft;
                if (currentAction != null)
                    QAIManager.Imitate(this, GetState(), currentAction);
            }
        }

        private bool Key(params KeyCode[] keys) {
            return keys.Any(kc => Input.GetKeyDown(kc));
        }
    }
}