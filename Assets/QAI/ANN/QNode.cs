using System;
using System.Collections.Generic;
using System.Linq;

public class QNode {
    private readonly List<QNode> outgoing;
    private readonly List<double> weights;
    private double value, error;
    private int saturation = 0, arity = 0;

    public double Signal { get; private set; }

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

    public void Activate(double x) {
        value += x;
        if (++saturation >= arity) {
            Signal = arity > 0 ? Sigmoid(value) : value;
            for (int i = 0; i < outgoing.Count; i++)
                outgoing[i].Activate(Signal * weights[i]);
            value = 0;
            saturation = 0;
        }
    }

    // For output neurons.
    public double CalculateError(double target) {
        return error = Signal * (1 - Signal) * (target - Signal);
    }

    // For hidden and input neurons.
    public double CalculateError() {
        return error = Signal * (1 - Signal) * outgoing.Select((n, i) => n.error * weights[i]).Sum();
    }

    public void AdjustWeights(double lrate) {
        for (int i = 0; i < outgoing.Count; i++)
            weights[i] += lrate * outgoing[i].error * Signal;
    }

    private double Sigmoid(double t) {
        return 1 / (1 + Math.Exp(-t));
    }

    public IEnumerable<double> GetWeights() {
        return weights;
    }
}
