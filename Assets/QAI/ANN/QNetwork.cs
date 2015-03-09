using System;
using System.Collections.Generic;
using System.Linq;

public class QNetwork {
    private readonly QNode[] input, output;
    private readonly QNode[][] hidden;

    public QNetwork(int inputSize, int outputSize, int numHiddenLayers, int hiddenLayerSize) {
        input = new QNode[inputSize];
        output = new QNode[outputSize];
        hidden = new QNode[numHiddenLayers][];
        // Input layer.
        for (int i = 0; i < inputSize; i++)
            input[i] = new QNode();
        // Hidden layers.
        QNode[] prevLayer = input;
        for (int n = 0; n < numHiddenLayers; n++) {
            hidden[n] = new QNode[hiddenLayerSize];
            for (int i = 0; i < hiddenLayerSize; i++) {
                hidden[n][i] = new QNode();
                for (int j = 0; j < prevLayer.Length; j++)
                    prevLayer[j].Connect(hidden[n][i]);
            }
            prevLayer = hidden[n];
        }
        // Output layer.
        output = new QNode[outputSize];
        for (int i = 0; i < outputSize; i++) {
            output[i] = new QNode();
            for (int j = 0; j < prevLayer.Length; j++)
                prevLayer[j].Connect(output[i]);
        }
    }

    public void Feedforward(double[] values) {
        for (int i = 0; i < values.Length; i++)
            input[i].Activate(values[i]);
    }

    public void Backpropagate(double[] targets) {
        for (int i = 0; i < output.Length; i++)
            output[i].CalculateError(targets[i]);
        for (int n = hidden.Length - 1; n >= 0; n--)
            for (int i = 0; i < hidden[n].Length; i++) {
                hidden[n][i].CalculateError();
                hidden[n][i].AdjustWeights();
            }
        for (int i = 0; i < input.Length; i++) {
            input[i].CalculateError();
            input[i].AdjustWeights();
        }
    }

    public IEnumerable<double> Output() {
        return output.Select(n => n.Signal);
    }
}
