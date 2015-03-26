using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour {
    
    private static int _index = 0;
    public List<Vector3> GoalPositions;
    public bool arbitrarySpawn;
    
    public static Goal Instance;
    public static Vector3 Position;
    public static int[] State;

    public void Awake() {
        if (GoalPositions.Count > 0 && arbitrarySpawn) {
            transform.position = GoalPositions[_index];
            _index++;
            if (_index >= GoalPositions.Count) _index = 0;
        }

        Instance = this;
        Position = transform.position;
        State = new[] {Mathf.RoundToInt(Position.x), 1, Mathf.RoundToInt(Position.z)};
    }
}
