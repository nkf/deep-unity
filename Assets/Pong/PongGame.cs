using UnityEngine;

namespace Pong {
    class PongGame : MonoBehaviour {
    
        //Set in editor
        public Bounds Border;

        private readonly int[] _score = new int[2];

        public int GetScore(Player p) {
            return _score[(int) p];
        }

        public void Score(Player p) {
            _score[(int) p]++;
        }


        void Update () {
            DebugDrawBounds(Border, Color.yellow);
        }

        public static void DebugDrawBounds(Bounds b, Color c) {
            var min = b.min; var max = b.max;
            var nw = new Vector3(min.x, min.y);
            var ne = new Vector3(max.x, min.y);
            var sw = new Vector3(min.x, max.y);
            var se = new Vector3(max.x, max.y);
            Debug.DrawLine(nw, ne, c);
            Debug.DrawLine(ne, se, c);
            Debug.DrawLine(se, sw, c);
            Debug.DrawLine(sw, nw, c);
        }

        public static void DebugDrawBounds(Bounds b) {
            DebugDrawBounds(b, Color.white);
        }

        public static Bounds BoundsFromTransform(Transform t) {
            var s = t.localScale;
            var p = t.position;
            var center = new Vector3(p.x, p.y);
            var size = new Vector3(s.x, s.y);
            return new Bounds(center, size);
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
