using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class PongBenchmark : QTester {

    private float y = -1;
    public override bool SetupNextTest(QAgent agent) {
        FindObjectOfType<PongBall>().Reset(new Vector2(-1, y));
        y += 0.2f;
        return y <= 1;
    }

    public override void OnActionTaken(QAgent agent, SARS sars) {
        
    }

    readonly List<double> _rewards = new List<double>(); 
    public override void OnTestComplete(double reward) {
        _rewards.Add(reward);
        Debug.Log("Run Complete");
    }

    public override void OnRunComplete() {
        Debug.Log(string.Join(",", _rewards.Select(d => d.ToString(CultureInfo.InvariantCulture)).ToArray()) +" ~ "+ _rewards.Sum() + "/" + _rewards.Count);
    }
}
