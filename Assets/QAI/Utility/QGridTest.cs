using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace QAI.Utility {
    public class QGridTest : MonoBehaviour {
        private QGrid _grid;

        private const int X = 5;
        private const int Y = 4;
        private const float Resolution = 1.5f;

        private void Start() {
            //_grid = new QGrid(X, Y, 1, transform, Resolution);
            Time.timeScale = 10f;
            //StartCoroutine(TestBounds());
            //Test1(new Vector3(3.00f,3.65f));
            StartCoroutine(TestValues());
        }

        private IEnumerator TestBounds() {
            var ex = _grid.Bounds.extents.x;
            var ey = _grid.Bounds.extents.y;
            var t = new Func<float, float, YieldInstruction>((x, y) => {
                var c = _grid.Locate(new Vector3(x, y));
                Debug.Log(string.Format("{0:F},{1:F} = {2}", x, y, c.HasValue ? c.ToString() : "outside grid"));
                if (c.HasValue) _grid[c.Value]++;
                Debug.DrawLine(new Vector3(x, y), new Vector3(x, y - 0.1f), Color.red);
                return new WaitForFixedUpdate();
            });
            var tests = new IEnumerator[4];
            tests[0] = Loop( ex,  ey, 0.1f, t);
            tests[1] = Loop( ex, -ey, 0.1f, t);
            tests[2] = Loop(-ex,  ey, 0.1f, t);
            tests[3] = Loop(-ex, -ey, 0.1f, t);
            foreach (var test in tests) {
                while (test.MoveNext()) yield return test.Current;
            }

            foreach (var d in _grid.Matrix.EnumerateRows()) {
                Debug.Log(d);
            }
            EditorApplication.isPlaying = false;
        }

        private IEnumerator Loop(float tx, float ty, float m, Func<float,float,YieldInstruction> t) {
            for(float x = tx - m; x < tx + m; x += 0.01f) {
                for(float y = ty - m; y < ty + m; y += 0.01f) {
                    yield return t(x, y);
                }
            }
        }

        private void Test1(Vector3 p) {
            var c = _grid.Locate(p);
            if(c.HasValue) _grid[c.Value]++;
            EditorApplication.isPlaying = false;
        }

        private IEnumerator TestValues() {
            Coordinates? prev = null;
            var ex = _grid.Bounds.extents.x;
            var ey = _grid.Bounds.extents.y;
            for (var x = -ex; x < ex; x+=0.1f) {
                for (var y = -ey; y < ey; y+=0.01f) {
                    var c = _grid.Locate(new Vector3(x,y));
                    if(prev.HasValue) _grid[prev.Value] = 0;
                    if(c.HasValue) _grid[c.Value] = 1;
                    prev = c;
                    Debug.DrawLine(new Vector3(x, y), new Vector3(x, y - 0.1f), Color.red);
                    yield return new WaitForFixedUpdate();
                }
            }
            EditorApplication.isPlaying = false;
        }


        private void Update() {
            _grid.DebugDraw(d => d > 0 ? Color.green : Color.white);
        }
    }
}