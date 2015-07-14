﻿using System.Collections.Generic;
using System.Linq;
using GridProto;
using QAI;
using UnityEngine;

public class Goal : MonoBehaviour {
    public enum SpawnTechnique {
        UseGoalPosistions, RandomAllValid, RandomAllValidAlsoWoman, None
    }
    public SpawnTechnique Technique;
    
    //Goal Positions
    private static int _index = 0;
    public List<Vector3> GoalPositions;

    //Random All Valid
    private static List<Vector3> _randomGoalPositions = null;

    //Random All Valid Also Woman
    private static List<Vector3> _randomWomanPositions = null; 
    
    public static Goal Instance;
    public static Vector3 Position;

    void Awake() {
        if(FindObjectOfType<QAIManager>().Mode == QAIMode.Testing) 
            Technique = SpawnTechnique.None;
        if (Technique == SpawnTechnique.UseGoalPosistions && GoalPositions.Count > 0) {
            transform.position = GoalPositions[_index];
            _index++;
            if (_index >= GoalPositions.Count) _index = 0;
        } else if (Technique == SpawnTechnique.RandomAllValid || Technique == SpawnTechnique.RandomAllValidAlsoWoman) {
            if (_randomGoalPositions == null || _randomGoalPositions.Count == 0) _randomGoalPositions = AllValidPositions();
            PickPlaceRemove(_randomGoalPositions,transform, new Vector3(0,-0.5f,0));
            if (Technique == SpawnTechnique.RandomAllValidAlsoWoman) {
                if (_randomWomanPositions == null || _randomWomanPositions.Count == 0) _randomWomanPositions = AllValidPositions();
                PickPlaceRemove(_randomWomanPositions, FindObjectOfType<GridWoman>().transform, Vector3.zero);
            }
        }

        Instance = this;
        Position = transform.position;
    }

    private void PickPlaceRemove(List<Vector3> list, Transform target, Vector3 offset) {
        var pos = list.Random().First();
        target.position = pos + offset;
        list.Remove(pos);
    }

    const int MinX = -20, MinY = -20, MaxX = 20, MaxY = 20;
    public static List<Vector3> AllValidPositions() {
        var positions = new List<Vector3>();
        for (var x = MinX; x < MaxX; x++) {
            for (var y = MinY; y < MaxY; y++) {
                var position = new Vector3(x, 1, y);
                if(Physics.Raycast(position, Vector3.down, 2f)) positions.Add(position);
            }
        }
        return positions;
    }
}
