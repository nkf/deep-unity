using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class QLearningFA : QLearning {

    private readonly QAgent _agent;
    private readonly QApx _apx;

    public QLearningFA(QAgent agent, int numFeatures) {
        _agent = agent;
        _apx = new QApx(_agent.GetQActions(), numFeatures);
    }



    public IEnumerator Learn(int iteration) {
        var s = _agent.GetState();
        var apx = _apx.Copy();
        while (!s.IsTerminal) {
            var a = _apx.Policy(s);
            a.Invoke();
            var sn = _agent.GetState();
            var r = s.Reward; 
            apx.Update(a,r,s, _agent.GetQActions(), sn);
            yield return new WaitForEndOfFrame();
        }

        //QAI.Restart();
    }
}
