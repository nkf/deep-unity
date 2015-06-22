using UnityEngine;

namespace Pong {
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
            DebugDrawRect(Border, Color.yellow);
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
            return new Rect(p.x - s.x / 2, p.y - s.y / 2, s.x, s.y);
        }

        public static Rect Encapsulate(Rect r, Vector3 p) {
            var x = Mathf.Min(r.x, p.x);
            var y = Mathf.Max(r.y, p.y);
            var d = new Vector3(r.x, r.y) - p;
            var w = Mathf.Max(r.width,  Mathf.Abs(d.x));
            var h = Mathf.Max(r.height, Mathf.Abs(d.y));
            return new Rect(x,y,w,h);
        }
    }
}
