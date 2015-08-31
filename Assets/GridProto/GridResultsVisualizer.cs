using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GridProto {
    public class GridResultsVisualizer : MonoBehaviour {
        private readonly Color _win =  new Color(0.176f, 0.533f, 0.176f, 1f); 
        private readonly Color _lose = new Color(0.667f, 0.224f, 0.224f, 1f);
        private readonly Color _cycle = new Color(0.133f, 0.40f, 0.40f, 1f);

        void Start () {
            /*
            var path = Path.Combine("TestResults", QData.EscapeScenePath(EditorApplication.currentScene)) + ".xml";
            var results = new SerializableDictionary<Vector3, ResultPair>();
            QData.Load(path, results);
	    
            DrawResults(results);
            */
        } 

        public void DrawResults(IEnumerable<KeyValuePair<Vector3, ResultPair>> results) {
            var rewardLayer = new GameObject { name = "Reward" };
            rewardLayer.transform.parent = transform;
            rewardLayer.transform.position = new Vector3(0, -0.49f, 0);

            var distLayer = new GameObject { name = "DistanceScore" };
            distLayer.transform.parent = transform;
            distLayer.transform.position = new Vector3(0, 0.5f, 0);

            foreach(var result in results) {
                AddMarker((float)result.Value.Reward, result.Key, rewardLayer);
                //AddMarker((float)result.Value.DistScore, result.Key, distLayer);
            }
        }

        private void AddMarker(float value, Vector3 pos, GameObject layer) {
            var gobj = Instantiate(Resources.Load<GameObject>("ResultMarker"));
            gobj.transform.parent = layer.transform;
            gobj.transform.localPosition = pos;
            var c = _cycle;
            if (value > 0.9) c = _win;
            if (value < 0.1) c = _lose;
            gobj.GetComponent<Renderer>().material.color = c;
        }
    }
}
