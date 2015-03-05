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

    private readonly int[] GoalState = {2, 1, 4};
    public QState GetState() {
        var p = PositionToState(transform.position);
        var g = GoalState;

        var state = PosAndGoal(p, g);
        //var state = VectorToGoal(transform.position, new Vector3(GoalState[0], GoalState[1], GoalState[2]));
        var dead = !isAboveGround();
        //var goal = state.SequenceEqual(new []{0,0,0});
        var goal = p.SequenceEqual(g);
        return new QState(
            state,
            dead ? -1 : goal ? 1 : 0,
            dead || goal
        );
    }

    void Update() {

    }

    void Start() {
        QAI.Learn(this);

    }
}
