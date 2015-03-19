using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GridStateCreator : QTester {
    public int MinX, MinZ, MaxX, MaxZ;
    private int _x, _z;

    private readonly SerializableDictionary<Vector3, ResultPair> _results = new SerializableDictionary<Vector3, ResultPair>();
    private readonly List<double> _distScores = new List<double>();
    void Awake() {
        _x = MinX-1;
        _z = MinZ;
    }
    //TODO: RECONSIDER THIS DESIGN (mainly that agent is passed, what if multiple agent, what if other factors are relevant to state?)
    public override bool SetupNextState(QAgent agent) {
        var woman = agent as GridWoman;
        do {
            _x++;
            woman.transform.position = new Vector3(_x, 1, _z);
            if (_x > MaxX) {
                _x = MinX;
                _z++;
                if (_z > MaxZ) {
                    //WriteResults();
                    return false;
                }
            }
        } while (!woman.IsAboveGround());
        return true;
    }

    public override void OnActionTaken(QAgent agent, SARS sars) {
        var state = sars.NextState.Features;
        var distToGoal = new Vector2(state[0], state[1]).magnitude;
        _distScores.Add( 1/(distToGoal+1) );
    }

    public override void OnRunComplete(double reward) {
        _results[new Vector3(_x, 1, _z)] = new ResultPair{ Reward = reward, DistScore = _distScores.DefaultIfEmpty().Max() };
        _distScores.Clear();
        WriteResults();
    }

    private void WriteResults() {
        var path = Path.Combine("TestResults", QData.EscapeScenePath(EditorApplication.currentScene))+".xml";
        QData.Save(path, _results);
    }

}

public struct ResultPair {
    public double Reward { get; internal set; }
    public double DistScore { get; internal set; }
}
