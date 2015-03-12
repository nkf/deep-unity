using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class QNetwork : IEnumerable<QNode[]> {
    private readonly QNode[] input, output;
    private readonly QNode[][] hidden;
    private readonly Random rng = new Random();

    public QNetwork(int inputSize, int outputSize, int numHiddenLayers, int hiddenLayerSize, double[] weights = null) {
        int wi = 0;
        // Initialize arrays.
        input = new QNode[inputSize];
        output = new QNode[outputSize];
        hidden = new QNode[numHiddenLayers][];
        for (int i = 0; i < numHiddenLayers; i++)
            hidden[i] = new QNode[hiddenLayerSize];
        // Initialize array contents.
        foreach (var layer in this)
            for (int i = 0; i < layer.Length; i++)
                layer[i] = new QNode();
        // Build the network topology.
        var en = GetEnumerator();
        en.MoveNext();
        var prevLayer = en.Current;
        while (en.MoveNext()) {
            for (int i = 0; i < prevLayer.Length; i++)
                for (int j = 0; j < en.Current.Length; j++)
                    prevLayer[i].ConnectTo(en.Current[j], weights != null ? weights[wi++] : rng.NextDouble());
            prevLayer = en.Current;
        }
    }

    public void Feedforward(double[] values) {
        for (int i = 0; i < values.Length; i++)
            input[i].Activate(values[i]);
    }

    public void Backpropagate(double[] targets, double lrate) {
        for (int i = 0; i < output.Length; i++)
            output[i].CalculateError(targets[i]);
        for (int n = hidden.Length - 1; n >= 0; n--)
            for (int i = 0; i < hidden[n].Length; i++) {
                hidden[n][i].CalculateError();
                hidden[n][i].AdjustWeights(lrate);
            }
        for (int i = 0; i < input.Length; i++) {
            input[i].CalculateError();
            input[i].AdjustWeights(lrate);
        }
    }

    public IEnumerable<double> Output() {
        return output.Select(n => n.Signal);
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public IEnumerator<QNode[]> GetEnumerator() {
        yield return input;
        foreach (var layer in hidden)
            yield return layer;
        yield return output;
    }

    public void Save(string path) {
        FileStream fs = null;
        try {
            fs = File.Open(Path.Combine("QData", path), FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.AutoFlush = true;
            sw.WriteLine(string.Join(",", new[] {
                input.Length, output.Length, hidden.Length, hidden[0].Length
            }.Select(i => i.ToString()).ToArray()));
            sw.WriteLine(string.Join(",",
                this.SelectMany(l => l).SelectMany(n => n.GetWeights()).Select(w => w.ToString()).ToArray()
            ));
        } catch (IOException e) {
            Debug.Log(e);
        } finally {
            fs.Close();
        }
    }

    public static QNetwork Load(string path) {
        FileStream fs = null;
        try {
            fs = File.Open(Path.Combine("QData", path), FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            var delim = new[] { ',' };
            var line = sr.ReadLine().Split(delim);
            int input = int.Parse(line[0]);
            int output = int.Parse(line[1]);
            int hlayers = int.Parse(line[2]);
            int hsize = int.Parse(line[3]);
            double[] weights = sr.ReadLine().Split(delim).Select(s => double.Parse(s)).ToArray();
            return new QNetwork(input, output, hlayers, hsize, weights);
        } catch (IOException e) {
            Debug.Log(e);
            throw new Exception("Cannot load network from file " + path);
        } finally {
            fs.Close();
        }
    }
}
