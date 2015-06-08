﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

class PongController : MonoBehaviour, QAgent {
    public Vector3 StartPosistion { get; private set; }

    PongGame _game;
    PongBall _ball;

    Q2DGrid _grid;


    public int Hits { get; set; } //Set by pongball

    //Set in editor
    public Player Side = 0;
    readonly KeyCode[][] _keys = {
        new[] {KeyCode.W, KeyCode.S},
        new[] {KeyCode.UpArrow, KeyCode.DownArrow}
    };

    void Awake() {
        StartCoroutine(Movement());
        StartPosistion = transform.position;
        _game = FindObjectOfType<PongGame>();
        _ball = FindObjectOfType<PongBall>();
        //_grid = new QGrid(4, 10, 1, transform, new Vector3(10,0,0), 6f, 1.5f, 6f);
        _grid = new Q2DGrid(13, transform, new GridSettings{ Offset = new Vector3(6,0,0), ResolutionX = 1f});
    }


    void FixedUpdate() {
        if(Side == Player.Player1) _grid.DebugDraw(v => v == 1 ? Color.yellow : Color.magenta);
    }

    IEnumerator Movement() {
        var keys = _keys[(int)Side];
        while (true) {
            Action action = Idle;
            if (Input.GetKey(keys[0])) action = MoveUp;
            if (Input.GetKey(keys[1])) action = MoveDown;
            if(Side == Player.Player1) QAI.Imitate(this, action);
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
    
    private List<Coordinates2D?> _prevPositions = new List<Coordinates2D?>(); 
    public QState GetState() {
        var winner = _ball.IsTerminal();
        float reward;
        bool terminal;
        //var terminal = winner.HasValue;
        //var reward = terminal ? (winner.Value == Side ? 1 : 0) : 0;
        var b = PongGame.RectFromTransform(_ball.transform);
        var controller = PongGame.RectFromTransform(transform);
        controller.width += 0.2f;
        if (b.Overlaps(controller)) {
            reward = 1;
            terminal = winner.HasValue;
            terminal = true;
        } else {
            terminal = winner.HasValue;
            reward = terminal ? (winner.Value == Side ? 1 : 0) : 0;
        }
        

        //Calculate distance to top and bottom
        var topDist = _game.Border.yMax - controller.yMax;
        var botDist = controller.yMin - _game.Border.yMin;
        
        var bp = _ball.transform.position;
        var rbp = bp - transform.position;

        var nbp = bp + _ball.Velocity.normalized;
        var nbp1 = bp + _ball.Velocity.normalized * 2;
        var nbp2 = bp + _ball.Velocity.normalized * 3;
        var nbp3 = bp + _ball.Velocity.normalized * 4;

        var positions = new List<Coordinates2D?> { _grid.Locate(bp), _grid.Locate(nbp),
            _grid.Locate(nbp1), _grid.Locate(nbp2), _grid.Locate(nbp3) };

        SetGridValues(_grid, _prevPositions, 0);
        SetGridValues(_grid, positions, 1);
        _prevPositions = positions;

        var state = _grid.Matrix.Clone();
        
        /*
        var state = _grid.State
            .Concat(new double[]{rbp.x, rbp.y, topDist, botDist})
            .ToArray();
        */
        
        
//        var bv = _ball.Velocity;
//
//        var rbpn = rbp.normalized;
//        var rbpm = rbp.magnitude;
//
//        var bvn = bv.normalized;
//        var bvm = bv.magnitude;
//        var state =
//            _nv[rbpn.x]
//            .Concat(_nv[rbpn.y])
//            .Concat(_vm[rbpm])
//            .Concat(_nv[bvn.x])
//            .Concat(_nv[bvn.y])
//            .Concat(_vm[bvm])
//            .ToArray();
//            new double[] {rbpn.x, rbpn.y, rbpm/10, bvn.x, bvn.y, bvm/5};
        return new QState(state, reward, terminal);
    }

    private void SetGridValues(Q2DGrid grid, IEnumerable<Coordinates2D?> coords, float value) {
        foreach (var coord in coords.Where(c => c.HasValue).Select(c => c.Value)) {
            grid[coord] = value;
        }
    }
}
internal enum Player {
    Player1 = 0, Player2 = 1
}
