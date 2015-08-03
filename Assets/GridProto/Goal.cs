using System.Collections.Generic;
using System.Linq;
using GridProto;
using QAI;
using UnityEngine;

public class Goal : MonoBehaviour {
    public enum SpawnTechnique {
        None, UsePosistions, RandomAllValid
    }
    public SpawnTechnique GoalSpawn;
    public SpawnTechnique SeekerSpawn;
    public List<Vector3> GoalPositions;
    public List<Vector3> SeekerPositions;

    private enum Target {
        Goal, Seeker
    }

    private static Dictionary<Target, int> _targetIndex;
    private static Dictionary<Target, List<Vector3>> _targetPosistions;
    
    public static Goal Instance;
    public static Vector3 Position;

    void Awake() {
        var seeker = FindObjectOfType<GridWoman>();
        if (_targetIndex == null) {
            _targetIndex = new Dictionary<Target, int> {
                {Target.Goal, 0},
                {Target.Seeker, 0}
            };
        }
        if (_targetPosistions == null) {
            _targetPosistions = new Dictionary<Target, List<Vector3>> {
                {Target.Goal, GoalSpawn == SpawnTechnique.RandomAllValid ? AllValidPositions().Shuffle().ToList() : GoalPositions },
                {Target.Seeker, SeekerSpawn == SpawnTechnique.RandomAllValid ? AllValidPositions().Shuffle().ToList() : SeekerPositions }
            };
        }

        if (FindObjectOfType<QAIManager>().Mode != QAIMode.Testing) {
            SelectSpawn(transform, Target.Goal, GoalSpawn);
            SelectSpawn(seeker.transform, Target.Seeker, SeekerSpawn);
        }

        Instance = this;
        Position = new Vector3(transform.position.x,0,transform.position.z);
    }

    private void SelectSpawn(Transform transform, Target target, SpawnTechnique technique) {
        if (technique == SpawnTechnique.None) return;
        Spawn(transform, target);
    }


    private void Spawn(Transform targetTransform, Target target) {
        var positions = _targetPosistions[target];
        var index = _targetIndex[target];
        var pos = FixY(positions[index], target);
        targetTransform.position = pos;
        index++;
        if(index >= positions.Count) index = 0;
        _targetIndex[target] = index;
    }

    private Vector3 FixY(Vector3 v, Target target) {
        if(target == Target.Goal) return new Vector3(v.x,0.6f,v.z);
        if(target == Target.Seeker) return new Vector3(v.x, 1f, v.z);
        return v;
    }

    const int MinX = -20, MinY = -20, MaxX = 20, MaxY = 20;
    public static List<Vector3> AllValidPositions() {
        var positions = new List<Vector3>();
        for (var x = MinX; x < MaxX; x++) {
            for (var y = MinY; y < MaxY; y++) {
                var position = new Vector3(x, 1, y);
                if(Physics.Raycast(position, Vector3.down, 2f)) {
					positions.Add(new Vector3(x,0,y));
				}
            }
        }
        return positions;
    }
}
