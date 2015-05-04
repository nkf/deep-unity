using System;
using System.Linq;
using C5;
using UnityEngine;
using System.Collections;

class PongController : MonoBehaviour, QAgent {
    PongGame _game;
    PongBall _ball;

    QGrid _grid;
    //Ballposition x, y
    ContValue _bpx;
    ContValue _bpy;
    //Ballvelocity x, y
    ContValue _bvx;
    ContValue _bvy;

    //Set in editor
    public Player Side;
    readonly KeyCode[][] _keys = {
        new[] {KeyCode.W, KeyCode.S},
        new[] {KeyCode.UpArrow, KeyCode.DownArrow}
    };

    void Awake() {
        StartCoroutine(Movement());
        _game = FindObjectOfType<PongGame>();
        _ball = FindObjectOfType<PongBall>();
        _grid = new QGrid(5, 5, 1, transform, new Vector3(5,0,0), 2f);
        var interval = new float[]{-7, -4, -1, 1, 4, 7};
        _bpx = new ContValue(interval);
        _bpy = new ContValue(interval);
        _bvx = new ContValue(interval);
        _bvy = new ContValue(interval);
    }


    void Update() {
        //_grid.DebugDraw(v => v == 1 ? Color.blue : Color.magenta);
    }

    IEnumerator Movement() {
        var keys = _keys[(int)Side];
        while (true) {
            Action action = Idle;
            if (Input.GetKey(keys[0])) action = MoveUp;
            if (Input.GetKey(keys[1])) action = MoveDown;
            QAI.Imitate(this, action);
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
        pos.y = Mathf.Min(pos.y+h, _game.Border.yMax) - h;
        pos.y = Mathf.Max(pos.y-h, _game.Border.yMin) + h;
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

    private Coordinates? _prevBallPos = null;
    public QState GetState() {
        var winner = _ball.IsTerminal();
        var terminal = winner.HasValue;
        var reward = terminal ? (winner.Value == Side ? 1 : 0) : 0;
        //var reward = Vector3.Distance(_ball.transform.position, transform.position) < 1.25 ? 1 : 0;


        var bp = _ball.transform.position;
        var bpc = _grid.Locate(bp);
        if(_prevBallPos.HasValue) _grid[_prevBallPos.Value] = 0;
        if(bpc.HasValue) _grid[bpc.Value] = 1;
        _prevBallPos = bpc;
        var rbp = bp - transform.position;
        var bv = _ball.Velocity;
        var state = 
            //_grid.Grid
            //.Concat(_bpx[rbp.x]
            _bpx[rbp.x]
            .Concat(_bpy[rbp.y])
            .Concat(_bvx[bv.x])
            .Concat(_bvy[bv.y])
            .ToArray();
        return new QState(state, reward, terminal || reward == 1);
    }
}
internal enum Player {
    Player1 = 0, Player2 = 1
}
