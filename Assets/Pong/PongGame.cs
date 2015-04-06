﻿using UnityEngine;
using System.Collections;

class PongGame : MonoBehaviour {
    
    //Set in editor
    public Rect Border;

    private readonly int[] _score = new int[2];

    public int GetScore(Player p) {
        return _score[(int) p];
    }

    public void Score(Player p) {
        _score[(int) p]++;
    }


	void Update () {
	    DebugDrawRect(Border);
	}

    public static void DebugDrawRect(Rect r, Color c) {
        var nw = new Vector3(r.xMin, r.yMin);
        var ne = new Vector3(r.xMax, r.yMin);
        var sw = new Vector3(r.xMin, r.yMax);
        var se = new Vector3(r.xMax, r.yMax);
        Debug.DrawLine(nw, ne, c);
        Debug.DrawLine(ne, se, c);
        Debug.DrawLine(se, sw, c);
        Debug.DrawLine(sw, nw, c);
    }

    public static void DebugDrawRect(Rect r) {
        DebugDrawRect(r, Color.white);
    }

    public static Rect RectFromTransform(Transform t) {
        var s = t.localScale;
        var p = t.position;
        return new Rect(p.x - s.x / 2f, p.y - s.y / 2, s.x, s.y);
    }
}