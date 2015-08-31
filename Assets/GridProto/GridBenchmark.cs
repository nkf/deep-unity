using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QAI.Agent;
using QAI.Training;
using QAI.Utility;
using UnityEditor;
using UnityEngine;

namespace GridProto {
    public class GridBenchmark : QTester {
        
        //Set in editor
        public List<Vector3> Positions;

        private List<Vector3> _positions; 
        private SerializableDictionary<Vector3, ResultPair> _results;
        private List<double> _distScores;

        private Vector3 RunPosistion;
        public override void Init() {
			Time.timeScale = 8f;
            _results = new SerializableDictionary<Vector3, ResultPair>();
            _distScores = new List<double>();
			_positions = Goal.AllValidPositions ();
        }

        public override bool SetupNextTest(QAgent agent) {
            var visualizer = FindObjectOfType<GridResultsVisualizer>();
            visualizer.enabled = true;
            visualizer.DrawResults(_results);

            if (_positions.Count == 0) return false;
            RunPosistion = _positions[0];
            RunPosistion.y = 1;
            ((GridWoman)agent).transform.position = RunPosistion;
            _positions.RemoveAt(0);
            return true;
        }

        public override void OnActionTaken(QAgent agent, QAction action, QState state) {
            var distToGoal = (((GridWoman) agent).transform.position - Goal.Position).magnitude;
            _distScores.Add( 1/(distToGoal+1) );
        }

        public override void OnTestComplete(double reward) {
            if (FindObjectOfType<GridWoman>().DetectCycle()) reward = 0.5;
            _results[RunPosistion] = new ResultPair{ Reward = reward, DistScore = _distScores.DefaultIfEmpty().Max() };
            _distScores.Clear();
            //WriteResults();
        }

        public override void OnRunComplete() {
            var accuracy = _results.Select(p => p.Value.Reward > 0.9 ? 1 : 0).Average();
            var avgDistScore = _results.Select(r => r.Value.DistScore).Average();
            Debug.Log(string.Format("Accuracy: {0:P} Avg. Distance Score: {1:F}", accuracy, avgDistScore));
            BenchmarkSave.WriteGridResult(accuracy, avgDistScore);
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
}