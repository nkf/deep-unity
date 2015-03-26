using System;
using System.Collections;
using UnityEngine;

public abstract class QTester : MonoBehaviour {
    //TODO: RECONSIDER THIS DESIGN (mainly that agent is passed, what if multiple agent, what if other factors are relevant to state?)
    public abstract bool SetupNextState(QAgent agent);

    public abstract void OnActionTaken(QAgent agent, SARS sars);

    //TODO: implement different stats such as median and avg reward for the run, instead of just final reward.
    public abstract void OnRunComplete(double reward);
}
