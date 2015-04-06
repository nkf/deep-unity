using System;
using UnityEngine;
using System.Collections;

class PongController : MonoBehaviour, QAgent {
    PongGame _game;
    PongBall _ball;

    //Set in editor
    public Player Side;
    readonly KeyCode[][] _keys = {
        new[] {KeyCode.W, KeyCode.S},
        new[] {KeyCode.UpArrow, KeyCode.DownArrow}
    };

    void Start() {
        StartCoroutine(Movement());
        _game = FindObjectOfType<PongGame>();
        _ball = FindObjectOfType<PongBall>();
    }

    IEnumerator Movement() {
        var keys = _keys[(int)Side];
        while (true) {
            Action action = Idle;
            if (Input.GetKey(keys[0])) action = MoveUp;
            if (Input.GetKey(keys[1])) action = MoveDown;
            QAI.Imitate(this, action);
            yield return new WaitForEndOfFrame();
        }
    }

    const float SpeedModifer = 10;
    void Move(Vector3 direction) {
        //Move controller
        transform.position += direction * (Time.deltaTime * SpeedModifer);
        
        //Stay within game border
        var pos = transform.position;
        var h = transform.localScale.y/2f;
        pos.y = Mathf.Max(pos.y-h, _game.Border.yMax) + h;
        pos.y = Mathf.Min(pos.y+h, _game.Border.yMin) - h;
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
    public void Idle() { }

    public QState GetState() {
        var winner = _ball.IsTerminal();
        var terminal = winner.HasValue;
        var reward = terminal ? (winner.Value == Side ? 1 : 0) : 0;
        Debug.Log(terminal);
        var pos = transform.position;
        var bpos = _ball.transform.position;
        var bv = _ball.Velocity;
        var state = new double[] {
            pos.x, pos.y, bpos.x, bpos.y, bv.x, bv.y
        };
        return new QState(state, reward, terminal);
    }
}
internal enum Player {
    Player1 = 0, Player2 = 1
}
