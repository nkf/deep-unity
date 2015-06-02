using System;
using System.Linq;
using UnityEngine;
using System.Collections;

public class GridMan : MonoBehaviour, QAgent {

    private static readonly Vector3 Up =    new Vector3( 1, 0, 0);
    private static readonly Vector3 Down =  new Vector3(-1, 0, 0);
    private static readonly Vector3 Left =  new Vector3( 0, 0, 1);
    private static readonly Vector3 Right = new Vector3( 0, 0,-1);

    [QBehavior("NoWallUp")]
    private void MoveUp() {
        transform.position += Up;
    }
    [QBehavior("NoWallDown")]
    private void MoveDown() {
        transform.position += Down;
    }
    [QBehavior("NoWallLeft")]
    private void MoveLeft() {
        transform.position += Left;
    }
    [QBehavior("NoWallRight")]
    private void MoveRight() {
        transform.position += Right;
    }

    [QPredicate]
    private bool NoWallUp() {
        return !Physics.Raycast(transform.position, Up, 1f);
    }
    [QPredicate]
    private bool NoWallDown() {
        return !Physics.Raycast(transform.position, Down, 1f);
    }
    [QPredicate]
    private bool NoWallLeft() {
        return !Physics.Raycast(transform.position, Left, 1f);
    }
    [QPredicate]
    private bool NoWallRight() {
        return !Physics.Raycast(transform.position, Right, 1f);
    }


    public QState GetState() {
        var terminal = HP < 0;
        return new QState(
//            GetRelativeStateGrid().SelectMany(g => g).ToArray(),
//            terminal ? Score : 0,
//            terminal
            );
    }

    private const int AbsGridX = 30;
    private const int AbsGridY = 30;
    private double[][] GetAbsoluteStateGrid() {
        var grid = new double[AbsGridX][];
        for (var x = 0; x < AbsGridX; x++) {
            grid[x] = new double[AbsGridY];
            for (var y = 0; y < AbsGridY; y++) {
                grid[x][y] = AbsGridValue(x, y);
            }
        }
        return grid;
    }

    private double AbsGridValue(int x, int y) {
        RaycastHit hit;
        if(Physics.Raycast(new Vector3(x, 1, y), Vector3.down, out hit, 2f)) {
            if(hit.collider.GetComponent<GridMan>() != null) return 0;
            var lava = hit.collider.GetComponent<Lava>();
            if(lava == null) return 1;
            return lava.IsLethal ? 3 : 2;
        }
        return -1;
    }

    private const int RelGridX = 10;
    private const int RelGridY = 10;
    private double[][] GetRelativeStateGrid() {
        var pX = Mathf.RoundToInt(transform.position.x);
        var pY = Mathf.RoundToInt(transform.position.z);
        var grid = new double[RelGridX][];
        for(var x = 0; x < RelGridX; x++) {
            grid[x] = new double[RelGridY];
            for(var y = 0; y < RelGridY; y++) {
                grid[x][y] = RelGridValue(x+pX, y+pY);
            }
        }
        return grid;
    }
    private double RelGridValue(int x, int y) {
        RaycastHit hit;
        if(Physics.Raycast(new Vector3(x, 1, y), Vector3.down, out hit, 2f)) {
            var lava = hit.collider.GetComponent<Lava>();
            if(lava == null) return 1;
            return lava.IsLethal ? 30 : 20;
        }
        return -1;
    }

    

    public Action GetImitationAction() {
        throw new NotImplementedException();
    }

    public int Score { get; private set; }
    public int HP { get; private set; }

    private Action _action;

    // Use this for initialization
	void Start () {
	    HP = 5;
	    StartCoroutine(ScoreCoroutine());
	    StartCoroutine(HPCoroutine());
	    StartCoroutine(MovementCoroutine());
	}
    private IEnumerator ScoreCoroutine() {
        while (HP > 0) {
            Score++;
            yield return new WaitForSeconds(0.1f);
        }
    }
    private IEnumerator HPCoroutine() {
        while (HP > 0) {
            if (DamageTaken()) {
                HP--;
                yield return new WaitForSeconds(1);
            } else {
                yield return new WaitForEndOfFrame();
            }
        }
    }

    private bool DamageTaken() {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector3.down, out hit)) {
            var lava = hit.collider.GetComponent<Lava>();
            if(lava == null) return false;
            return lava.IsLethal;
        }
        return false;
    }

    private IEnumerator MovementCoroutine() {
        while (HP > 0) {
            _action = null;
            if(Input.GetKey(KeyCode.UpArrow)    && NoWallUp())      _action = MoveUp;
            if(Input.GetKey(KeyCode.DownArrow)  && NoWallDown())    _action = MoveDown;
            if(Input.GetKey(KeyCode.RightArrow) && NoWallRight())   _action = MoveRight;
            if(Input.GetKey(KeyCode.LeftArrow)  && NoWallLeft())    _action = MoveLeft;
            if (_action != null) {
                QAI.Imitate(this, _action);
                yield return new WaitForSeconds(0.10f);
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator TimeScaleCoroutine() {
        while (HP > 0) {
            Time.timeScale += 0.1f;
            Debug.Log(Time.timeScale);
            yield return new WaitForSeconds(1);
        }
        Time.timeScale = 0;
    }
}
