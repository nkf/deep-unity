using System;
using System.Collections.Generic;
using System.Linq;

public class QNode {
    private const double lrate = 1.0;

    private readonly List<QNode> outgoing;
    private readonly List<double> weights;
    private double value, signal, error;
    private int saturation = 0, arity = 0;

    public double Signal { get { return signal; } }

    public QNode() {
        outgoing = new List<QNode>();
        weights = new List<double>();
    }

    public void ConnectTo(QNode n, double w) {
        n.arity++;
        outgoing.Add(n);
        weights.Add(w);
    }

    public void ConnectFrom(QNode n, double w) {
        arity++;
        n.outgoing.Add(this);
        n.weights.Add(w);
    }

    public void Activate(double value) {
        this.value += value;
        if (++saturation >= arity) {
            signal = Sigmoid(value);
            for (int i = 0; i < outgoing.Count; i++)
                outgoing[i].Activate(signal * weights[i]);
            value = 0;
            saturation = 0;
        }
    }

    // For hidden and input neurons.
    public double CalculateError() {
        return error = signal * (1 - signal) * outgoing.Select((n, i) => n.error * weights[i]).Sum();
    }

    // For output neurons.
    public double CalculateError(double target) {
        return error = signal * (1 - signal) * (target - signal);
    }

    public void AdjustWeights() {
        for (int i = 0; i < outgoing.Count; i++)
            weights[i] += lrate * outgoing[i].error * signal;
    }

    private double Sigmoid(double t) {
        return 1 / (1 + Math.Exp(-t));
    }

    public IEnumerable<double> Weights() {
        return weights;
    }
}
