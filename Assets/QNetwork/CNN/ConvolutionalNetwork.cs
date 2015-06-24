using System.Collections.Generic;
using System.IO;
using System.Xml;
using MathNet.Numerics.LinearAlgebra;
using QNetwork.MLP;
using System.Collections.Generic;

namespace QNetwork.CNN {
	public class ConvolutionalNetwork : Unit<Matrix<float>[], Vector<float>> {
        // Forward propagation.
        private readonly SpatialLayer _input;
        private readonly ConvolutionalLayer[] _conv;
        private readonly MeanPoolLayer[] _subs;
        private readonly FlattenLayer _flatten;
        private readonly DenseLayer _output;

        // Backpropagation.
        private BackpropParams _params;
        private Vector<float> _loss;
        private List<Backprop<Matrix<float>[], Matrix<float>[]>> _backprop;
        private FlattenLayerBackprop _flatback;
        private DenseLayerBackprop _outback;

        // Serialization.
	    private readonly int _inroot, _labels;
	    private readonly CNNArgs[] _args;

        public ConvolutionalNetwork(int inroot, int labels, params CNNArgs[] args) {
            _inroot = inroot;
            _labels = labels;
            _args = args;
            _input = new SpatialLayer(inroot, 1); // TODO: Channels.
            _conv = new ConvolutionalLayer[args.Length];
            _subs = new MeanPoolLayer[args.Length];
            _conv[0] = new ConvolutionalLayer(args[0].FilterSize, args[0].FilterCount, args[0].Stride, _input, Functions.Tanh2D);
            _subs[0] = new MeanPoolLayer(args[0].PoolLayerSize, _conv[0]);
            for (int i = 1; i < args.Length; i++) {
                _conv[i] = new ConvolutionalLayer(args[i].FilterSize, args[i].FilterCount, args[i].Stride, _subs[i - 1], Functions.Tanh2D);
                _subs[i] = new MeanPoolLayer(args[i].PoolLayerSize, _conv[i]);
            }
            _flatten = new FlattenLayer(_subs[_subs.Length - 1]);
            _output = new DenseLayer(labels, _flatten, Functions.Sigmoid);
        }

        public int Size() {
            return _conv.Length + _subs.Length + 3;
        }

        public Vector<float> Compute(Matrix<float>[] input) {
            // Forward propagate.
            input = _input.Compute(input);
            for (int i = 0; i < _conv.Length; i++)
                input = _subs[i].Compute(_conv[i].Compute(input));
            return _output.Compute(_flatten.Compute(input));
        }

        public Vector<float> Output() {
            return _output.Output();
        }

        public void InitializeTraining(BackpropParams par) {
            _params = par;
            _loss = Vector<float>.Build.Dense(_output.Size());
            _backprop = new List<Backprop<Matrix<float>[], Matrix<float>[]>>();
            for (int i = _conv.Length - 1; i >= 0; i--) {
                _backprop.Add(new MeanPoolLayerBackprop(_subs[i]));
                _backprop.Add(new ConvolutionalLayerBackprop(_conv[i]));
            }
            _flatback = new FlattenLayerBackprop(_flatten);
            _outback = new DenseLayerBackprop(_output);
        }

        public void SGD(Matrix<float>[] features, Vector<float> labels) {
            Compute(features);
            labels.CopyTo(_loss);
            _loss.Subtract(Output(), _loss);
            var _e2d = _flatback.Visit(_outback.Visit(_loss, _params), _params);
            _backprop.BackPropagation(_e2d, _params);
        }

        public void SGD(Matrix<float>[] features, TargetIndexPair p) {
            _loss.Clear();
            _loss.At(p.Index, p.Target - Compute(features)[p.Index]);
            var _e2d = _flatback.Visit(_outback.Visit(_loss, _params), _params);
            _backprop.BackPropagation(_e2d, _params);
        }

		public IEnumerable<SpatialLayer> IterateSpatialLayers() {
			yield return _input;
			for(var i = 0; i < _conv.Length; i++) {
				yield return _conv[i];
				yield return _subs[i];
			}
		}

	    public void Save(string filename) {
            var file = File.Create(filename);
	        using (var writer = XmlWriter.Create(file, new XmlWriterSettings {Indent = true})) {
                writer.WriteStartElement(GetType().Name);
                //Constructor arguments
                writer.WriteElementString("inroot", _inroot.ToString());
                writer.WriteElementString("labels", _labels.ToString());
                writer.XmlSerialize(_args);
                //Layers
	            for (int i = 0; i < _conv.Length; i++)
	                _conv[i].Serialize(writer);
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
                var convl = reader.XmlDeserialize<CNNArgs[]>();
	            var network = new ConvolutionalNetwork(inroot, labels, convl);
                for (int i = 0; i < network._conv.Length; i++)
                    network._conv[i].Deserialize(reader);
                network._output.Deserialize(reader);
                
                reader.ReadEndElement();
	            return network;
	        }
	    }
	}
}
