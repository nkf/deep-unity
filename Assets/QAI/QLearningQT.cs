using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class QLearningQT : QLearning {
    private Param Epsilon = t => 0.2;
    private Param StepSize = t => 1.0 / t;
    private double Discount = 0.99;

    private const string QTableName = "QTable.xml";

    private QTable _QTable;

    public override IEnumerator<YieldInstruction> RunEpisode(QAI.EpisodeCallback callback) {
        Iteration++;
        yield return new WaitForEndOfFrame();
        var episode = Episode(callback);
        while (episode.MoveNext()) {
            yield return episode.Current;
        }
    }

    private IEnumerator<YieldInstruction> Episode(QAI.EpisodeCallback callback) {
        var s = Agent.GetState();
        while (!s.IsTerminal) {
            var a = EpsilonGreedy(Epsilon(Iteration));
            var q = Q(s)(a);
            a.Action.Invoke();
            var s0 = Agent.GetState();
            var a0max = Actions.Max(a0 => Q(s0)(a0));
            var v = q + StepSize(Iteration) * (s0.Reward + Discount * a0max - q);
            Update(s, a, v);
            s = s0;
            yield return new WaitForEndOfFrame();
        }
        callback();
    }

    public override void LoadModel() {
        _QTable.Load(QTableName);
    }

    public override void SaveModel() {
        _QTable.Save(QTableName);
    }

    public override void RemakeModel() {
        _QTable = new QTable(defaultReward: 1.0);
    }

    public override QLearning.ActionValueFunction Q(QState s) {
        return a => _QTable.Query(s, a);
    }

    private void Update(QState s, QAction a, double v) {
        _QTable.Add(s, a, v);
    }
}
