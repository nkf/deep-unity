using System;
using GridProto;
using QAI;
using UnityEditor;
using UnityEngine;

public class ModelTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
	    var positions = Goal.AllValidPositions();
	    var seeker = FindObjectOfType<GridWoman>();
	    foreach (var position in positions) {
	        seeker.transform.position = position;
	        var state = seeker.GetState();
	        var query = QAIManager.Query(state);
	        var max = query.MaxBy(kv => kv.Value);
	        var rotation = ActionToRotation(max.Key.ActionId);
            CreateArrow(position, rotation, max.Value);
	    }
	    EditorApplication.isPaused = true;
	}

    private void CreateArrow(Vector3 position, float rotation, float value) {
        var arrow = Instantiate(Resources.Load<GameObject>("Arrow"));
        arrow.transform.parent = transform;
        arrow.transform.position = position;
        arrow.transform.localEulerAngles = new Vector3(0,rotation,0);
        arrow.GetComponentInChildren<Renderer>().material.color = Color.Lerp(Color.red, Color.green, value);
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


}
