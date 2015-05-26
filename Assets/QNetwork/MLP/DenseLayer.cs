﻿using System;
using System.Xml;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.MLP {
    public class DenseLayer : Layer<Vector<float>> {
        private readonly Vector<float> _values;
        public ActivationFunction<Vector<float>> Activation { get; set; }
        public Vector<float> Biases { get; set; }
        public Matrix<float> Weights { get; set; }

        public DenseLayer(int size, Layer<Vector<float>> prev, ActivationFunction<Vector<float>> activation) {
            Activation = activation;
            var vb = Vector<float>.Build;
            var mb = Matrix<float>.Build;
            _values = vb.Dense(size);
            Biases = vb.Random(size, Normal.WithMeanStdDev(0.0, 0.05));
            Weights = mb.Random(size, prev.Size(), Normal.WithMeanStdDev(0.0, 0.5/Math.Sqrt(prev.Size())));
            Prev = prev;
        }

        public override int Size() {
            return Weights.RowCount;
        }

        public override Vector<float> Compute(Vector<float> input) {
            Weights.Multiply(input, _values);
            _values.Add(Biases, _values);
            Activation.Apply(_values, _values);
            return _values;
        }

        public override Vector<float> Output() {
            return _values;
        }

        public override T Accept<T>(Trainer<T> t, T state) {
            return t.Visit(this, state);
        }

        public override void Serialize(XmlWriter writer) {
            writer.WriteStartElement(GetType().Name);

            writer.XmlSerialize(Weights.ToColumnArrays());
            writer.XmlSerialize(Biases.ToArray());

            writer.WriteEndElement();
        }

        public override void Deserialize(XmlReader reader) {
            reader.ReadStartElement(GetType().Name);
            
            Weights = Matrix<float>.Build.DenseOfColumnArrays( reader.XmlDeserialize<float[][]>() );
            Biases = Vector<float>.Build.Dense( reader.XmlDeserialize<float[]>() );

            reader.ReadEndElement();
        }
    }
}