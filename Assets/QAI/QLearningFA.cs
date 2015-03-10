using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class QLearningFA : QLearning {

    private const string QApxName = "QApx.xml";

    private readonly QAgent _agent;
    private readonly QApx _apx;

    public QLearningFA(QAgent agent, QApx apx = null) {
        _agent = agent;
        if(apx == null) {
            _apx = new QApx(_agent.GetQActions(), _agent.GetState().Features.Length);
            _apx.Load(QApxName);
        } else {
            _apx = apx;
        }
    }

    public IEnumerator Learn(int iteration) {
        var s = _agent.GetState();
        var t = 1;
        while (!s.IsTerminal) {
            var a = _apx.Policy(s, _agent.GetQActions().Where(qa => qa.IsValid()), t);
            a.Invoke();
            var sn = _agent.GetState();
            var r = sn.Reward; 
            _apx.SaveUpdate(a,r,s, _agent.GetQActions(), sn, t);
            s = sn;
            t++;
            yield return new WaitForEndOfFrame();
        }
        _apx.CompleteUpdate();
        if(iteration % 10 == 0)
            _apx.Save(QApxName);
        QAI.Restart(_apx);
    }
}
