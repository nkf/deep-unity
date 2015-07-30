using System;
using System.Collections.Generic;
using System.Linq;
using QAI.Agent;
using QAI.Training;
using QAI.Utility;
using UnityEngine;

namespace Pong {
    public class PongBenchmark : QTester {
        public static bool Running = false;
        private List<TestScore> _scores;
        private int _iteration;
        private float y;
        public override void Init() {
            y = -1;
            _iteration = -1;
            _scores = new List<TestScore>();
        }

        public override bool SetupNextTest(QAgent agent) {
            Running = true;
            Time.timeScale = 3;
            _scores.Add(new TestScore());
            _iteration++;
            FindObjectOfType<PongBall>().Reset(new Vector2(-1, y));
            y += 0.2f;
            if (Math.Abs(y) < 0.01) //y == 0
                y += 0.2f;
            return y <= 1;
        }

        public override void OnActionTaken(QAgent agent, SARS sars) {}

        
        public override void OnTestComplete(double reward) {
            var winner = FindObjectOfType<PongBall>().IsTerminal();
            if (winner.HasValue) {
                if (winner.Value == Player.Player1) _scores[_iteration].GameWon = true;
                else _scores[_iteration].BallDistance = DistanceToBall();
            }
            var pad = FindObjectsOfType<PongController>().First(pc => pc.Side == Player.Player1);
            _scores[_iteration].Hits = pad.Hits;
        }

        public override void OnRunComplete() {
            Running = false;
            var hits = _scores.Select(ts => ts.Hits).Sum();
            var wins = _scores.Select(ts => ts.GameWon ? 1 : 0).Sum();
            var avgDist = _scores.Select(ts => ts.BallDistance).Average();
            BenchmarkSave.WritePongResult(hits, wins, avgDist);
            Debug.Log(string.Format("Hits:{0:D} AvgDist:{1:F3} Wins:{2:D}",hits, avgDist, wins));
        }

        private float DistanceToBall() {
            var ball = FindObjectOfType<PongBall>();
            var pad = FindObjectsOfType<PongController>().First(pc => pc.Side == Player.Player1);
            return (ball.transform.position - pad.transform.position).magnitude;
        }
        
        private class TestScore {
            public int Hits;
            public double BallDistance;
            public bool GameWon = false;
        }
    }
}
