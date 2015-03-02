using System;
using System.Linq;
using UnityEngine;

public class GridWoman : MonoBehaviour, QAgent {

    private bool isAboveGround() {
        return Physics.Raycast(transform.position, Vector3.down);
    }

    private void MoveUp() {
        transform.position += new Vector3(1,0,0);
    }
    private void MoveDown() {
        transform.position += new Vector3(-1,0,0);
    }
    private void MoveLeft() {
        transform.position += new Vector3(0,0,1);
    }
    private void MoveRight() {
        transform.position += new Vector3(0,0,-1);
    }

    public Action[] GetActions() {
        return new Action[] {MoveRight, MoveUp, MoveDown, MoveLeft};
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
