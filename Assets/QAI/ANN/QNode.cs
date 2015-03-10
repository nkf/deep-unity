using System;
using System.Collections.Generic;
using System.Linq;

public class QNode {
    private const double lrate = 1.0;

    private readonly List<QNode> connections;
    private readonly List<double> weights;
    private double value, signal, error;
    private int saturation = 0;
    private readonly Random rng = new Random();

    public double Signal { get { return signal; } }

    public QNode() {
        connections = new List<QNode>();
        weights = new List<double>();
    }

    public void Connect(QNode n) {
        connections.Add(n);
        weights.Add(rng.NextDouble());
    }

    public void Activate(double value) {
        this.value += value;
        if (++saturation == connections.Count) {
            signal = Sigmoid(value);
            connections.ForEach(n => n.Activate(signal));
            value = 0;
            saturation = 0;
        }
    }

    // For hidden and input neurons.
    public double CalculateError() {
        return error = signal * (1 - signal) * connections.Select((n, i) => n.error * weights[i]).Sum();
    }

    // For output neurons.
    public double CalculateError(double target) {
        return error = signal * (1 - signal) * (target - signal);
    }

    public void AdjustWeights() {
        for (int i = 0; i < connections.Count; i++)
            weights[i] += lrate * connections[i].error * signal;
    }

    private double Sigmoid(double t) {
        return 1 / (1 + Math.Exp(-t));
    }
}
