using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class QLearningQT : QLearning {

    private const string QTableName = "QTable.xml";

    private QTable _QTable;

    public QLearningQT(QAgent agent) : base(agent) { }

    public override QAction BestAction() {
        throw new NotImplementedException();
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

    protected override double Q(QState s, QAction a) {
        return _QTable.Query(s, a);
    }

    protected override void Update(QState s, QAction a, double v) {
        _QTable.Add(s, a, v);
    }
}
