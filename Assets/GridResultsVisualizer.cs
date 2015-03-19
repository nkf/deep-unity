using System.IO;
using UnityEditor;
using UnityEngine;

public class GridResultsVisualizer : MonoBehaviour {
    private readonly Color _win =  new Color(0,1,1,0.2f); //cyan
    private readonly Color _lose = new Color(1,0,0,0.2f); //red

	void Start () {
        var path = Path.Combine("TestResults", QData.EscapeScenePath(EditorApplication.currentScene)) + ".xml";
	    var results = new SerializableDictionary<Vector3, ResultPair>();
	    QData.Load(path, results);
	    
        var rewardLayer = new GameObject{name = "Reward"};
	    rewardLayer.transform.parent = transform;
        rewardLayer.transform.position = new Vector3(0,0.25f,0);

        var distLayer = new GameObject{name = "DistanceScore"};
        distLayer.transform.parent = transform;
        distLayer.transform.position = new Vector3(0,0.5f,0);

	    foreach (var result in results) {
            AddMarker((float)result.Value.Reward, result.Key, rewardLayer);
            AddMarker((float)result.Value.DistScore, result.Key, distLayer);
	    }
	}

    private void AddMarker(float value, Vector3 pos, GameObject layer) {
        var gobj = Instantiate(Resources.Load<GameObject>("ResultMarker"));
        gobj.transform.parent = layer.transform;
        gobj.transform.localPosition = pos;
        var c = Color.Lerp(_lose, _win, value);
        gobj.GetComponent<Renderer>().material.color = c;
    }
}
