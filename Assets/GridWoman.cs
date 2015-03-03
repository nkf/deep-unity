using System;
using System.Linq;
using UnityEngine;

public class GridWoman : MonoBehaviour, QAgent {

    private bool isAboveGround() {
        return Physics.Raycast(transform.position, Vector3.down);
    }

    [QBehavior()]
    private void MoveUp() {
        transform.position += new Vector3(1,0,0);
    }
    [QBehavior()]
    private void MoveDown() {
        transform.position += new Vector3(-1,0,0);
    }
    [QBehavior()]
    private void MoveLeft() {
        transform.position += new Vector3(0,0,1);
    }
    [QBehavior()]
    private void MoveRight() {
        transform.position += new Vector3(0,0,-1);
    }

    private int[] PositionToState(Vector3 p) {
        return new [] {
            Mathf.RoundToInt(p.x),
            Mathf.RoundToInt(p.y),
            Mathf.RoundToInt(p.z),
        };
    }

    private readonly int[] GoalState = {0, 1, 5};
    public QState GetState() {
        var state = PositionToState(transform.position);
        var dead = !isAboveGround();
        var goal = GoalState.SequenceEqual(state);
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
