using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class QLearningFA : QLearning {

    private const string QApxName = "QApx.xml";

    private QApx _apx;

    public QLearningFA(QAgent agent) : base(agent) { }

    public override void LoadModel() {
        _apx.Load(QApxName);
    }

    public override void SaveModel() {
        _apx.Save(QApxName);
    }

    public override void RemakeModel() {
        _apx = new QApx(Actions, Agent.GetState().Features.Length);
    }

    protected override double Q(QState s, QAction a) {
        return _apx.Q(s, a);
    }

    protected override void Update(QState s, QAction a, double v) {
        throw new NotImplementedException();
    }

    public QAction Policy(QState s, IEnumerable<QAction> actions, int t) {
        if(Roll() < Epsilon(t)) {
            var qActions = actions.ToArray();
            return qActions[Roll(qActions.Length)];
        }
        return actions
            .Select(a => new { q = Q(s, a), a })
            .MaxWithRandomTie(x => x.q).a;
    }

    protected override IEnumerator<YieldInstruction> Episode(QAI.EpisodeCallback callback) {
        var s = Agent.GetState();
        var apx = _apx.Copy();
        var t = 1;
        while (!s.IsTerminal) {
            var a = Policy(s, Agent.GetQActions().Where(qa => qa.IsValid()), t);
            a.Invoke();
            var sn = Agent.GetState();
            var r = sn.Reward;
            apx.Update(a, r, s, Agent.GetQActions(), sn, t);
            s = sn;
            t++;
            yield return new WaitForEndOfFrame();
        }
        _apx = apx;
        callback();
    }
}
