using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using QAI.Agent;
using QAI.Training;
using QAI.Utility;
using UnityEngine;

namespace Pong {
    public class PongBenchmark : QTester {
        readonly List<TestScore> _scores = new List<TestScore>();
        private int _iteration = -1;
        private float y = -1;
        public override bool SetupNextTest(QAgent agent) {
            _scores.Add(new TestScore());
            _iteration++;
            FindObjectOfType<PongBall>().Reset(new Vector2(-1, y));
            y += 0.2f;
            return y <= 1;
        }

        public override void OnActionTaken(QAgent agent, SARS sars) {
            if (sars.Reward == 1f) {
                if (DistanceToBall() < 5f) {
                    _scores[_iteration].Hits++;
                }
            }
        }

        
        public override void OnTestComplete(double reward) {
            var winner = FindObjectOfType<PongBall>().IsTerminal();
            if (winner.HasValue) {
                if (winner.Value == Player.Player1) _scores[_iteration].GameWon = true;
                else _scores[_iteration].BallDistance = DistanceToBall();
            }
        }

        public override void OnRunComplete() {
            var hits = _scores.Select(ts => ts.Hits).Sum();
            var wins = _scores.Select(ts => ts.GameWon ? 1 : 0).Sum();
            var avgDist = _scores.Select(ts => ts.BallDistance).Average();
            BenchmarkSave.WritePongResult(hits, wins, avgDist);
            Debug.Log(string.Format("Hits:{0:D} AvgDist:{1:F} Wins:{2:D}",hits, avgDist, wins));
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
