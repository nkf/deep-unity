using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridWoman : MonoBehaviour, QAgent {
    
    private Q2DGrid _grid;
    private Bin _nvectorBin;
    private void Start() {
        _grid = new Q2DGrid(12, transform, new GridSettings { NormalAxis = Axis.Y });
        _nvectorBin = new Bin(-0.75f,-0.25f,0.25f,0.75f);
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
            Mathf.RoundToInt(p.y),
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
        var g = Goal.State;
        //var state = PosAndGoal(p, g);
        //var state = PosToGoal(transform.position, Goal.Position);
        //var state = VectorToGoal(transform.position, new Vector3(GoalState[0], GoalState[1], GoalState[2]));
        /*
        double[, ,] grids = new double[1, 9, 9];
        for (int n = 0; n < 1; n++) {
            for (int i = 0; i < 9; i++) {
                for (int j = 0; j < 9; j++) {
                    var x = i + p[0] - 4;
                    var z = j + p[2] - 4;
                    var ray = new Ray(new Vector3(x, 2, z), Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 3.0f)) {
                        if (n == 0) {
                            if (hit.collider.gameObject == Goal.Instance.gameObject)
                                grids[n, i, j] = 10.0;
                            else if (hit.collider.gameObject != this.gameObject)
                                grids[n, i, j] = 1.0;
                        } else {
                            if (hit.collider.gameObject != this.gameObject)
                                grids[n, i, j] = 1.0;
                        }
                    }
                }
            }
        }*/
        _grid.Populate(bounds => {
            var ray = new Ray(new Vector3(bounds.center.x, 2, bounds.center.z), Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 3.0f)) {
                return hit.collider.gameObject == Goal.Instance.gameObject ? 10 : 1;
            }
            return 0;
        });
        var dead = !IsAboveGround();
        var goal = p.SequenceEqual(g);
        var v = VectorToGoal(transform.position, Goal.Position).normalized;
        return new QState(
            /*
            _grid.State
                 .Concat(new double[] {v.x, v.z})
                 .ToArray(),
            */
            null,
            //_grid.Matrix.Clone(),
            dead ? 0 : goal ? 1 : 0,
            dead || goal
            );
    }


    public void Update() {
        _grid.DebugDraw(value => value == 0 ? Color.red : value == 1 ? Color.gray : Color.yellow);
        Action currentAction = null;
        if (Key(KeyCode.UpArrow, KeyCode.W)) currentAction = MoveUp;
        if (Key(KeyCode.DownArrow, KeyCode.S)) currentAction = MoveDown;
        if (Key(KeyCode.RightArrow, KeyCode.D)) currentAction = MoveRight;
        if (Key(KeyCode.LeftArrow, KeyCode.A)) currentAction = MoveLeft;
        if (currentAction != null)
            QAI.Imitate(this, currentAction);
    }

    private bool Key(params KeyCode[] keys) {
        return keys.Any(kc => Input.GetKeyDown(kc));
    }
}