using System;
using System.Collections;
using UnityEngine;

public abstract class QTester : MonoBehaviour {
    public abstract bool SetupNextTest(QAgent agent);

    public abstract void OnActionTaken(QAgent agent, SARS sars);

    public abstract void OnTestComplete(double reward);

    public abstract void OnRunComplete();
}
