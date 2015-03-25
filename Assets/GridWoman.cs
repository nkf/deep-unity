using System;
using System.Linq;
using UnityEngine;

public class GridWoman : MonoBehaviour, QAgent {

    public bool IsAboveGround() {
        return Physics.Raycast(transform.position, Vector3.down);
    }

    [QBehavior("NoLeft")]
    private void NoMove() {
        // Do nothing.
    }
    [QBehavior("NoLeft")]
    private void MoveUp() {
        transform.position += new Vector3(1,0,0);
    }
    [QBehavior("NoLeft")]
    private void MoveDown() {
        transform.position += new Vector3(-1,0,0);
    }
    [QBehavior("NoLeft")]
    private void MoveLeft() {
        transform.position += new Vector3(0,0,1);
    }
    [QBehavior("NoLeft")]
    private void MoveRight() {
        transform.position += new Vector3(0,0,-1);
    }

    [QPredicate]
    private bool NoLeft(Action a) {
        return true;
    }

    private int[] PositionToState(Vector3 p) {
        return new [] {
            Mathf.RoundToInt(p.x),
            Mathf.RoundToInt(p.y),
            Mathf.RoundToInt(p.z),
        };
    }

    private int[] VectorToGoal(Vector3 p, Vector3 goal) {
        var v = goal - p;
        return PositionToState(v);
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


    private readonly Vector3 Min = new Vector3(-1,0,-4);
    private int[] Offset(Vector3 p) {
        return PositionToState(p - Min);
    }

    private readonly int[] GoalState;
    public QState GetState() {
        var p = PositionToState(transform.position);
        var g = Goal.State;
        //var state = PosAndGoal(p, g);
		//var state = PosToGoal(transform.position, Goal.Position);
        //var state = VectorToGoal(transform.position, new Vector3(GoalState[0], GoalState[1], GoalState[2]));
        double[, ,] grids = new double[2, 9, 9];
        for (int n = 0; n < 2; n++) {
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
                        } else {
                            if (hit.collider.gameObject != this.gameObject)
                                grids[n, i, j] = 1.0;
                        }
                    }
                }
            }
        }
        var dead = !IsAboveGround();
        var goal = p.SequenceEqual(g);
        return new QState(
            grids.Cast<double>().ToArray(),
            dead ? 0 : goal ? 1 : 0,
            dead || goal
        );
    }

	private Action _currentAction;
	public Action GetImitationAction() {
		return _currentAction;
	}

	public void Update() {
		_currentAction = null;
		if(Input.GetKeyDown(KeyCode.UpArrow))
			_currentAction = MoveUp;
		if(Input.GetKeyDown(KeyCode.DownArrow))
			_currentAction = MoveDown;
		if(Input.GetKeyDown(KeyCode.RightArrow))
			_currentAction = MoveRight;
		if(Input.GetKeyDown(KeyCode.LeftArrow))
			_currentAction = MoveLeft;
		if(_currentAction != null)
			QAI.Imitate(this);
	}
}
