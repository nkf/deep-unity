using System;
using System.Linq;
using UnityEngine;

public class GridWoman : MonoBehaviour, QAgent {

    private bool isAboveGround() {
        return Physics.Raycast(transform.position, Vector3.down);
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
		var v = VectorToGoal(p, goal);
		var s = PositionToState(p);
		return new[] {v[0], v[2], s[0], s[2]};
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
		var state = PosToGoal(transform.position, Goal.Position);
        //var state = VectorToGoal(transform.position, new Vector3(GoalState[0], GoalState[1], GoalState[2]));
        var dead = !isAboveGround();
        //var goal = state.SequenceEqual(new []{0,0,0});
        var goal = p.SequenceEqual(g);
        return new QState(
            state.Select(i => (double)i).ToArray(),
            dead ? -1 : goal ? 1 : 0.0,
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
