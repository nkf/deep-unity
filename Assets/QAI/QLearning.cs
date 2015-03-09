using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Assets.QAI;
using UnityEngine;
using Random = System.Random;

abstract class QLearning {
    public const double TIE_BREAK = 1e-9;

    protected delegate double param(int i);
    protected param Epsilon { get { return t => 0.2; } }
    protected param StepSize { get { return t => 1.0 / t; } }
    protected param Discount { get { return t => 1.0 / t; } }

    private readonly Random _rng;
    private QAgent _agent;
    private IList<QAction> _actions;
    private int _it = 0;

    public QAgent Agent { get { return _agent; } }
    public IList<QAction> Actions { get { return _actions; } }
    public int Iteration { get { return _it; } }

    public QLearning() {
        _rng = new Random();
    }

    public IEnumerator<YieldInstruction> RunEpisode(QAgent agent) {
        _it++;
        var _agent = agent;
        var _actions = agent.GetQActions();
        return Episode();
    }

    protected virtual IEnumerator<YieldInstruction> Episode() {
        var s = Agent.GetState();
        while (!s.IsTerminal) {
            var a = Policy(s);
            var q = Q(s, a);
            a.Action.Invoke();
            var s0 = Agent.GetState();
            var a0max = Actions.Max(a0 => Q(s0, a0));
            var v = q + StepSize(Iteration) * (s0.Reward + Discount(Iteration) * a0max - q);
            Update(s, a, v);
            s = s0;
            yield return new WaitForEndOfFrame();
        }
    }

    protected virtual QAction Policy(QState s) {
        if (Roll() < Epsilon(Iteration)) return Actions[Roll(Actions.Count)];
        return Actions.Where(a => a.IsValid()).OrderByDescending(a => Q(s, a) + Roll() * TIE_BREAK).First();
    }

    protected abstract double Q(QState s, QAction a);
    protected abstract void Update(QState s, QAction a, double v);

    public abstract void SaveModel();

    protected int Roll(int bound) {
        return _rng.Next(bound);
    }

    protected double Roll() {
        return _rng.NextDouble();
    }
}

/*class QLearning {
    private delegate double param(int i);

    private readonly QTable _QTable;
    private readonly QAgent _agent;
    private readonly Random _rng;

    private const string QTablePath = "QData/JOHN.xml";

    public QLearning(QAgent agent, QTable table = null) {
        _agent = agent;
        _rng = new Random();
        
        if (table == null) {
            _QTable = new QTable(1); // Default reward of 1.0
            _QTable.Load(QTablePath);
        } else {
            _QTable = table;
        }
    }

    private double Q(QState s, QAction a) {
        return _QTable.Query(s, a);
    }
    
    private param eps = t => 0.8;
    private QAction EpsilonGreedy(QState s, QAction[] actions, int t) {
        if (_rng.NextDouble() < eps(t)) return actions[_rng.Next(actions.Length)];
        var ordered = actions.Select(a => new {A = a, Q = Q(s, a)}).OrderByDescending(x => x.Q);
        var highest = ordered.First().Q;
        var highArray = ordered.Where(x => x.Q == highest).ToArray();
        return highArray[_rng.Next(highArray.Length)].A;
    }

    private param discount = t => 1.0 / t;
    private param stepsize = t => 1.0 / t;
    public IEnumerator Learn(int iteration) {
        var t = 1;
        var s = _agent.GetState();
        var actions = _agent.GetQActions();
        while (!s.IsTerminal) {
            var a = EpsilonGreedy(s, actions.Where(ac => ac.IsValid()).ToArray(), t);
            a.Action.Invoke();
            var s0 = _agent.GetState();
            var a0max = actions.Max(a0 => Q(s0, a0));
            var v = Q(s, a) + stepsize(t) * (s0.Reward + discount(t) * a0max - Q(s, a));
            _QTable.Add(s,a,v);
            s = s0;
            t++;
            yield return new WaitForEndOfFrame();
        }
        if (iteration%10 == 0)
            _QTable.Save(QTablePath);
        QAI.Restart(_QTable);
    }

    private const string Path = "JOHN.xml";
    private void SaveQTable(SerializableDictionary<QState, SerializableDictionary<QAction, double>> qTable) {
        XmlWriter writer = null;
        try {
            writer = XmlWriter.Create(File.Open(Path, FileMode.Create));
            qTable.WriteXml(writer);
        }
        catch (Exception e) {
            Debug.Log(e);
        }
        finally {
            if (writer != null) writer.Close();
        }
    }
 
    private SerializableDictionary<QState, SerializableDictionary<QAction, double>> ReadQTable() {
        var fileStream = File.Open(Path, FileMode.OpenOrCreate);
        var reader = XmlReader.Create(fileStream);
        var qTable = new SerializableDictionary<QState, SerializableDictionary<QAction, double>>();
        try {
            qTable.ReadXml(reader);
            return qTable;
        }
        catch (Exception e) {
            Debug.Log(e);
            return new SerializableDictionary<QState, SerializableDictionary<QAction, double>>();
        }
        finally {
            fileStream.Close();
            reader.Close();
        }
    }
}
*/