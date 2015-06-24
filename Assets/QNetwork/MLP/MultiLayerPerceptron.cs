using System.Collections.Generic;
using System.IO;
using System.Xml;
using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.MLP {
    public class MultiLayerPerceptron : Unit<Vector<float>, Vector<float>> {
        // Forward propagation.
        private readonly InputLayer _input;
        private readonly DenseLayer[] _hidden;
        private readonly DenseLayer _output;

        // Backpropagation.
        private BackpropParams _params;
        private Vector<float> _loss;
        private List<Backprop<Vector<float>, Vector<float>>> _backprop;

        // Serialization.
        private readonly int _insize;
        private readonly int[] _layers;

        public MultiLayerPerceptron(int insize, params int[] layers) {
            _insize = insize;
            _layers = layers;
            _input = new InputLayer(insize);
            _hidden = new DenseLayer[layers.Length - 1];
            _hidden[0] = new DenseLayer(layers[0], _input, Functions.Sigmoid);
            for (int i = 1; i < _hidden.Length; i++)
                _hidden[i] = new DenseLayer(layers[i], _hidden[i - 1], Functions.Sigmoid);
            _output = new DenseLayer(layers[layers.Length - 1], _hidden[_hidden.Length - 1], Functions.Sigmoid);
        }

        public int Size() {
            return _hidden.Length + 2;
        }

        public Vector<float> Compute(Vector<float> input) {
            return _output.Compute(_hidden.ForwardPropagation(_input.Compute(input)));
        }

        public Vector<float> Output() {
            return _output.Output();
        }

        public void InitializeTraining(BackpropParams par) {
            _params = par;
            _loss = Vector<float>.Build.Dense(_output.Size());
            _backprop = new List<Backprop<Vector<float>, Vector<float>>>();
            _backprop.Add(new DenseLayerBackprop(_output));
            for (int i = _hidden.Length - 1; i >= 0; i--)
                _backprop.Add(new DenseLayerBackprop(_hidden[i]));
        }

        public void SGD(Vector<float> features, Vector<float> labels) {
            Compute(features);
            labels.CopyTo(_loss);
            _loss.Subtract(Output(), _loss);
            _backprop.BackPropagation(_loss, _params);
        }

        public void SGD(Vector<float> features, TargetIndexPair p) {
            _loss.Clear();
            _loss.At(p.Index, p.Target - Compute(features)[p.Index]);
            _backprop.BackPropagation(_loss, _params);
        }

        public void Save(string filename) {
            var file = File.Create(filename);
            using (var writer = XmlWriter.Create(file, new XmlWriterSettings { Indent = true })) {
                writer.WriteStartElement(GetType().Name);
                //Constructor arguments
                writer.WriteElementString("insize", _insize.ToString());
                writer.XmlSerialize(_layers);
                //Layers
                for (int i = 0; i < _hidden.Length; i++)
                    _hidden[i].Serialize(writer);
                _output.Serialize(writer);
                writer.WriteEndElement();
            }
            file.Close();
        }

        public static MultiLayerPerceptron Load(string filename) {
            using (var reader = XmlReader.Create(File.Open(filename, FileMode.Open))) {
                reader.ReadStartElement(typeof(MultiLayerPerceptron).Name);
                var insize = int.Parse(reader.ReadElementString());
                var layers = reader.XmlDeserialize<int[]>();
                var network = new MultiLayerPerceptron(insize, layers);
                for (int i = 0; i < network._hidden.Length; i++)
                    network._hidden[i].Deserialize(reader);
                network._output.Deserialize(reader);
                reader.ReadEndElement();
                return network;
            }
        }
    }
}
