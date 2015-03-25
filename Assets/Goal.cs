using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour {

    public static Goal Instance;

    public static Vector3 Position;
    public static int[] State;
    void Awake() {
        Instance = this;
        Position = transform.position;
        State = new[] {Mathf.RoundToInt(Position.x), 1, Mathf.RoundToInt(Position.z)};
    }
}
