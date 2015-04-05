using System;
using UnityEngine;
using System.Collections;

class PongController : MonoBehaviour {
    PongGame _game;

    //Set in editor
    public Player Side;
    readonly KeyCode[][] _keys = {
        new[] {KeyCode.W, KeyCode.S},
        new[] {KeyCode.UpArrow, KeyCode.DownArrow}
    };

    void Start() {
        StartCoroutine(Movement());
        _game = FindObjectOfType<PongGame>();
    }

    IEnumerator Movement() {
        var keys = _keys[(int)Side];
        while (true) {
            Action action = null;
            if (Input.GetKey(keys[0])) action = MoveUp;
            if (Input.GetKey(keys[1])) action = MoveDown;
            if (action != null) action();
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
    void MoveUp() {
        Move(Vector3.up);
    }
    void MoveDown() {
        Move(Vector3.down);
    }

	
}
internal enum Player {
    Player1 = 0, Player2 = 1
}
