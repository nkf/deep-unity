using System;
using System.Xml;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.MLP {
    public class DenseLayer : Layer<Vector<float>> {
        private readonly Vector<float> _values;
        public ActivationFunction<Vector<float>> Activation { get; private set; }
        public Vector<float> Biases { get; private set; }
        public Matrix<float> Weights { get; private set; }

        public DenseLayer(int size, Layer<Vector<float>> prev, ActivationFunction<Vector<float>> activation) {
            Activation = activation;
            var vb = Vector<float>.Build;
            var mb = Matrix<float>.Build;
            _values = vb.Dense(size);
            //Biases = vb.Random(size, Normal.WithMeanStdDev(0.0, 0.05));
            Biases = vb.Dense(size, Functions.BiasInitValue(activation));
            Weights = mb.Random(size, prev.Size(), Normal.WithMeanStdDev(0.0, Functions.WeightInitStdDev(prev.Size(), activation)));
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
