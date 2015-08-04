using System;
using System.Collections.Generic;
using System.Linq;
using QAI;
using UnityEditor;
using UnityEngine;

namespace GridProto {
    public class ModelTest : MonoBehaviour {

        // Use this for initialization
        void Start () {
            IEnumerable<Vector3> positions = Goal.AllValidPositions();
            var seeker = FindObjectOfType<GridWoman>();
            var objMap = new Dictionary<Vector3, GameObject>();
            var dirMap = new Dictionary<Vector3, Vector3>();
			positions = positions.Where (p => !p.Equals (Goal.Position));
            foreach (var position in positions) {
				var pos = position + new Vector3(0,1,0);
                seeker.transform.position = pos;
                var state = seeker.GetState();
                var query = QAIManager.Query(state);
                var max = query.MaxBy(kv => kv.Value);
                var rotation = ActionToRotation(max.Key.ActionId);
                var arrow = CreateArrow(pos, rotation, max.Value);
                objMap.Add(pos, arrow);
                dirMap.Add(pos, FollowAction(pos, max.Key.ActionId));
            }
            //Find cycles
            foreach (var cyclePos in dirMap.Where(kv => dirMap.ContainsKey(kv.Value) && kv.Key == dirMap[kv.Value])) {
                objMap[cyclePos.Key].GetComponentInChildren<Renderer>().material.color = Color.cyan;
            }
	    

            EditorApplication.isPaused = true;
        }

        private GameObject CreateArrow(Vector3 position, float rotation, float value) {
            var arrow = Instantiate(Resources.Load<GameObject>("Arrow"));
            arrow.transform.parent = transform;
            arrow.transform.position = position;
            arrow.transform.localEulerAngles = new Vector3(0,rotation,0);
            arrow.GetComponentInChildren<Renderer>().material.color = Color.Lerp(Color.red, Color.green, value);
            return arrow;
        }

        private float ActionToRotation(string actionName) {
            switch (actionName) {
                case "MoveUp":
                    return 270;
                case "MoveLeft":
                    return 180;
                case "MoveDown":
                    return 90;
                case "MoveRight":
                    return 0;
                default:
                    throw new Exception("Unexcepted action name");
            }
        }

        private Vector3 FollowAction(Vector3 pos, string actionName) {
            switch (actionName) {
                case "MoveUp":
                    return pos + new Vector3(1, 0, 0);
                case "MoveLeft":
                    return pos + new Vector3(0, 0, 1);
                case "MoveDown":
                    return pos + new Vector3(-1, 0, 0);
                case "MoveRight":
                    return pos + new Vector3(0, 0, -1);
                default:
                    throw new Exception("Unexcepted action name");
            }
        }
    }
}
