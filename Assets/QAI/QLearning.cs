using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using Random = System.Random;

class QLearning {
    private readonly SerializableDictionary<QState, SerializableDictionary<QAction, double>> _qTable;
    private readonly QAgent _agent;
    private readonly Random _rng;
    public QLearning(QAgent agent) {
        _agent = agent;
        _rng = new Random();
        _qTable = ReadQTable();
        Debug.Log(_qTable.Count);
    }

    private double Q(QState s, QAction a) {
        if (!_qTable.ContainsKey(s)) return 0;
        var qa = _qTable[s];
        if (!qa.ContainsKey(a)) return 0;
        return qa[a];
    }

    private void QAdd(QState s, QAction a, double v) {
        SerializableDictionary<QAction, double> qa;
        if (!_qTable.ContainsKey(s)) {
            qa = new SerializableDictionary<QAction, double>();
            _qTable.Add(s, qa);
        } else {
            qa = _qTable[s];
        }
        qa[a] = v;
    }


    private const double Epsilon = 1;
    private QAction EpsilonGreedy(QState s, QAction[] actions) {
        if (_rng.NextDouble() > Epsilon) return actions[_rng.Next(actions.Length)];
        var ordered = actions.Select(a => new {A = a, Q = Q(s, a)}).OrderByDescending(x => x.Q);
        var highest = ordered.First().Q;
        var highArray = ordered.Where(x => x.Q == highest).ToArray();
        return highArray[_rng.Next(highArray.Length)].A;
    }

    private const double Discount = 0.5;
    public IEnumerator Learn() {
        var t = 1;
        var s = _agent.GetState();
        while (!s.IsTerminal) {
            var a = EpsilonGreedy(s, _agent.GetQActions());
            a.Action.Invoke();
            var s0 = _agent.GetState();
            var a0max = _agent.GetQActions().Max(a0 => Q(s0, a0));
            var v = Q(s, a) + 1.0/t * (s0.Reward + Discount * a0max - Q(s, a));
            QAdd(s,a,v);
            s = s0;
            t++;
            yield return new WaitForEndOfFrame();
        }
        SaveQTable(_qTable);
        Application.LoadLevel(0);
    }

    private const string Path = "JOHN.xml";
    private void SaveQTable(SerializableDictionary<QState, SerializableDictionary<QAction, double>> qTable) {
        XmlWriter writer = null;
        try {
            writer = XmlWriter.Create(File.Open(Path, FileMode.Create));;
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
