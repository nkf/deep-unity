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

        private SerializableDictionary<Vector3, ResultPair> _results;
        private List<double> _distScores;

        private Vector3 RunPosistion;

        public override void Init() {
            _results = new SerializableDictionary<Vector3, ResultPair>();
            _distScores = new List<double>();
        }

        public override bool SetupNextTest(QAgent agent) {
            FindObjectOfType<GridResultsVisualizer>().enabled = true;
            if (Positions.Count == 0) return false;
            RunPosistion = Positions[0];
            RunPosistion.y = 1;
            ((GridWoman)agent).transform.position = RunPosistion;
            Positions.RemoveAt(0);
            return true;
        }

        public override void OnActionTaken(QAgent agent, SARS sars) {
            var state = sars.NextState.Features;
            //var distToGoal = new Vector2((float)state[0], (float)state[1]).magnitude;
            //_distScores.Add( 1/(distToGoal+1) );
        }

        public override void OnTestComplete(double reward) {
            if (reward == 0) reward = 0.5;
            _results[RunPosistion] = new ResultPair{ Reward = reward, DistScore = _distScores.DefaultIfEmpty().Max() };
            _distScores.Clear();
            WriteResults();
        }

        public override void OnRunComplete() {
            var accuracy = _results.Select(p => p.Value.Reward > 0.9 ? 1 : 0).Average();
            Debug.Log(string.Format("{0:P} accuracy, {1:D} samples",accuracy, _results.Count));
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