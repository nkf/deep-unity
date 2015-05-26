﻿using System.IO;
using System.Xml;
using MathNet.Numerics.LinearAlgebra;
using QNetwork.MLP;

namespace QNetwork.CNN {
	public class ConvolutionalNetwork : Unit<Matrix<float>[], Vector<float>> {
        private readonly SpatialLayer _input;
        private readonly SpatialLayer[] _hidden;
        private readonly FlattenLayer _flatten;
        private readonly DenseLayer _output;

	    private readonly int _inroot, _labels;
	    private readonly int[][] _convl;

        public ConvolutionalNetwork(int inroot, int labels, params int[][] convl) {
            _inroot = inroot;
            _labels = labels;
            _convl = convl;
            _input = new SpatialLayer(inroot, 1); // TODO: Channels.
            _hidden = new SpatialLayer[convl.Length * 2];
            _hidden[0] = new ConvolutionalLayer(convl[0][0], convl[0][1], convl[0][2], _input, Functions.Tanh2D);
            _hidden[1] = new MeanPoolLayer(convl[0][3], _hidden[0]);
            for (int i = 2; i < _hidden.Length; i += 2) {
                _hidden[i] = new ConvolutionalLayer(convl[i][0], convl[i][1], convl[i][2], _hidden[i - 1], Functions.Tanh2D);
                _hidden[i + 1] = new MeanPoolLayer(convl[i][3], _hidden[i]);
            }
            _flatten = new FlattenLayer(_hidden[_hidden.Length - 1]);
            _output = new DenseLayer(labels, _flatten, Functions.Softmax);
        }

        public int Size() {
            return (_hidden.Length / 2) + 3;
        }

        public Vector<float> Compute(Matrix<float>[] input) {
            // Forward propagate.
            return _output.Compute(_flatten.Compute(_hidden.ForwardPropagation(_input.Compute(input))));
        }

        public Vector<float> Output() {
            return _output.Output();
        }

        public T Accept<T>(Trainer<T> t, T state) {
            // Backpropagate.
            return _input.Accept(t, _hidden.ApplyTrainer(t, _flatten.Accept(t, _output.Accept(t, state))));
        }

	    public void Save(string filename) {
            var file = File.Create(filename);
	        using (var writer = XmlWriter.Create(file, new XmlWriterSettings {Indent = true})) {
                writer.WriteStartElement(GetType().Name);
                //Constructor arguments
                writer.WriteElementString("inroot", _inroot.ToString());
                writer.WriteElementString("labels", _labels.ToString());
                writer.XmlSerialize(_convl);
                //Layers
	            for (int i = 0; i < _hidden.Length; i += 2)
	                _hidden[i].Serialize(writer);
	            _output.Serialize(writer);

                writer.WriteEndElement();
	        }
            file.Close();
	    }

	    public static ConvolutionalNetwork Load(string filename) {
	        using (var reader = XmlReader.Create(File.Open(filename, FileMode.Open))) {
	            reader.ReadStartElement(typeof(ConvolutionalNetwork).Name);
	            
                var inroot = int.Parse(reader.ReadElementString());
	            var labels = int.Parse(reader.ReadElementString());
                int[][] convl = new int[0][];
                reader.XmlDeserialize(ref convl);
	            var network = new ConvolutionalNetwork(inroot, labels, convl);
                for (int i = 0; i < network._hidden.Length; i += 2)
                    network._hidden[i].Deserialize(reader);
                network._output.Deserialize(reader);
                
                reader.ReadEndElement();
	            return network;
	        }
	    }
	}
}