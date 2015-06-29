using System.Collections.Generic;
using System.IO;
using System.Xml;
using MathNet.Numerics.LinearAlgebra;
using QNetwork.MLP;
using System.Collections.Generic;

namespace QNetwork.CNN {
	public class ConvolutionalNetwork : Unit<Matrix<float>[], Vector<float>> {
        // Forward propagation.
        public SpatialLayer InputLayer { get; private set; }
        public ConvolutionalLayer[] ConvolutionalLayers { get; private set; }
        public MeanPoolLayer[] SubSampleLayers { get; private set; }
        public FlattenLayer FlattenLayer { get; private set; }
        public DenseLayer OutputLayer { get; private set; }

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
            InputLayer = new SpatialLayer(inroot, 1); // TODO: Channels.
            ConvolutionalLayers = new ConvolutionalLayer[args.Length];
            SubSampleLayers = new MeanPoolLayer[args.Length];
            ConvolutionalLayers[0] = new ConvolutionalLayer(args[0].FilterSize, args[0].FilterCount, args[0].Stride, InputLayer, Functions.Tanh2D);
            SubSampleLayers[0] = new MeanPoolLayer(args[0].PoolLayerSize, ConvolutionalLayers[0]);
            for (int i = 1; i < args.Length; i++) {
                ConvolutionalLayers[i] = new ConvolutionalLayer(args[i].FilterSize, args[i].FilterCount, args[i].Stride, SubSampleLayers[i - 1], Functions.Tanh2D);
                SubSampleLayers[i] = new MeanPoolLayer(args[i].PoolLayerSize, ConvolutionalLayers[i]);
            }
            FlattenLayer = new FlattenLayer(SubSampleLayers[SubSampleLayers.Length - 1]);
            OutputLayer = new DenseLayer(labels, FlattenLayer, Functions.Sigmoid);
        }

        public int Size() {
            return ConvolutionalLayers.Length + SubSampleLayers.Length + 3;
        }

        public Vector<float> Compute(Matrix<float>[] input) {
            // Forward propagate.
            input = InputLayer.Compute(input);
            for (int i = 0; i < ConvolutionalLayers.Length; i++)
                input = SubSampleLayers[i].Compute(ConvolutionalLayers[i].Compute(input));
            return OutputLayer.Compute(FlattenLayer.Compute(input));
        }

        public Vector<float> Output() {
            return OutputLayer.Output();
        }

        public void InitializeTraining(BackpropParams par) {
            _params = par;
            _loss = Vector<float>.Build.Dense(OutputLayer.Size());
            _backprop = new List<Backprop<Matrix<float>[], Matrix<float>[]>>();
            for (int i = ConvolutionalLayers.Length - 1; i >= 0; i--) {
                _backprop.Add(new MeanPoolLayerBackprop(SubSampleLayers[i]));
                _backprop.Add(new ConvolutionalLayerBackprop(ConvolutionalLayers[i]));
            }
            _flatback = new FlattenLayerBackprop(FlattenLayer);
            _outback = new DenseLayerBackprop(OutputLayer);
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
			yield return InputLayer;
			for(var i = 0; i < ConvolutionalLayers.Length; i++) {
				yield return ConvolutionalLayers[i];
				yield return SubSampleLayers[i];
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
	            for (int i = 0; i < ConvolutionalLayers.Length; i++)
	                ConvolutionalLayers[i].Serialize(writer);
	            OutputLayer.Serialize(writer);

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
                for (int i = 0; i < network.ConvolutionalLayers.Length; i++)
                    network.ConvolutionalLayers[i].Deserialize(reader);
                network.OutputLayer.Deserialize(reader);
                
                reader.ReadEndElement();
	            return network;
	        }
	    }
	}
}
