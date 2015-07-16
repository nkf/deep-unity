using System.Collections.Generic;
using System.IO;
using System.Xml;
using MathNet.Numerics.LinearAlgebra;
using QAI.Agent;
using QNetwork.Experimental;
using QNetwork.MLP;

namespace QNetwork.CNN {
    public class ConvolutionalNetwork : Unit<StatePair, Vector<float>> {
        // Forward propagation.
        public SpatialLayer InputLayer { get; private set; }
        public ConvolutionalLayer[] ConvolutionalLayers { get; private set; }
        public MeanPoolLayer[] SubSampleLayers { get; private set; }
        public FlattenLayer FlattenLayer { get; private set; }
        public TreeLayer CombinationLayer { get; private set; }
        public DenseLayer OutputLayer { get; private set; }

        public bool IsOutputFromTraining { get; private set; }

        private VectorPair _vecp;

        // Backpropagation.
        private BackpropParams _params;
        private Vector<float> _loss;
        private List<Backprop<Matrix<float>[], Matrix<float>[]>> _backprop;
        private FlattenLayerBackprop _unflatten;
        private TreeLayerBackprop _split;
        private DenseLayerBackprop _outback;

        // Serialization.
        private readonly int _matsize, _vecsize, _labels, _depth;
	    private readonly CNNArgs[] _args;

        public ConvolutionalNetwork(int matsize, int vecsize, int depth, int labels, params CNNArgs[] args) {
            _matsize = matsize;
            _vecsize = vecsize;
            _depth = depth;
            _labels = labels;
            _args = args;
            InputLayer = new SpatialLayer(matsize, depth);
            ConvolutionalLayers = new ConvolutionalLayer[args.Length];
            SubSampleLayers = new MeanPoolLayer[args.Length];
            ConvolutionalLayers[0] = new ConvolutionalLayer(args[0].FilterSize, args[0].FilterCount, args[0].Stride, InputLayer, Functions.Rectifier2D);
            SubSampleLayers[0] = new MeanPoolLayer(args[0].PoolLayerSize, ConvolutionalLayers[0]);
            for (int i = 1; i < args.Length; i++) {
                ConvolutionalLayers[i] = new ConvolutionalLayer(args[i].FilterSize, args[i].FilterCount, args[i].Stride, SubSampleLayers[i - 1], Functions.Rectifier2D);
                SubSampleLayers[i] = new MeanPoolLayer(args[i].PoolLayerSize, ConvolutionalLayers[i]);
            }
            FlattenLayer = new FlattenLayer(SubSampleLayers[SubSampleLayers.Length - 1]);
            CombinationLayer = new TreeLayer(FlattenLayer.Size(), vecsize);
            OutputLayer = new DenseLayer(labels, CombinationLayer, Functions.Identity);
        }

        public int Size() {
            return ConvolutionalLayers.Length + SubSampleLayers.Length + 3;
        }

        public Vector<float> Compute(StatePair input) {
            // Forward propagate.
            var img = InputLayer.Compute(input.Spatial);
            for (int i = 0; i < ConvolutionalLayers.Length; i++)
                img = SubSampleLayers[i].Compute(ConvolutionalLayers[i].Compute(img));
            _vecp.left = FlattenLayer.Compute(img);
            _vecp.right = input.Linear;
            IsOutputFromTraining = false;
            return OutputLayer.Compute(CombinationLayer.Compute(_vecp));
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
            _unflatten = new FlattenLayerBackprop(FlattenLayer);
            _split = new TreeLayerBackprop(CombinationLayer);
            _outback = new DenseLayerBackprop(OutputLayer);
        }

        public void SGD(StatePair input, Vector<float> labels) {
            Compute(input);
            labels.CopyTo(_loss);
            _loss.Subtract(Output(), _loss);
            var img = _split.Visit(_outback.Visit(_loss, _params), _params).left;
            _backprop.BackPropagation(_unflatten.Visit(img, _params), _params);
            IsOutputFromTraining = true;
        }

        public void SGD(StatePair input, TargetIndexPair p) {
            _loss.Clear();
            _loss.At(p.Index, p.Target - Compute(input)[p.Index]);
            var img = _split.Visit(_outback.Visit(_loss, _params), _params).left;
            _backprop.BackPropagation(_unflatten.Visit(img, _params), _params);
            IsOutputFromTraining = true;
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
                writer.WriteElementString("matsize", _matsize.ToString());
                writer.WriteElementString("vecsize", _vecsize.ToString());
                writer.WriteElementString("labels", _labels.ToString());
                writer.WriteElementString("depth", _depth.ToString());
                writer.XmlSerialize(_args);
                //Layers
                foreach (var conv in ConvolutionalLayers)
                    conv.Serialize(writer);
	            OutputLayer.Serialize(writer);
                writer.WriteEndElement();
	        }
            file.Close();
	    }

	    public static ConvolutionalNetwork Load(string filename, bool oldnetwork = false) {
            var file = File.Open(filename, FileMode.Open);
	        using (var reader = XmlReader.Create(file)) {
	            reader.ReadStartElement(typeof(ConvolutionalNetwork).Name);
                var matsize = int.Parse(reader.ReadElementString());
	            var vecsize = int.Parse(reader.ReadElementString());
                var labels = int.Parse(reader.ReadElementString());
                int depth = 1;
                if (!oldnetwork) {
                    try {
                        depth = int.Parse(reader.ReadElementString());
                    } catch (XmlException e) {
                        // For backwards compatibility with depth-1 networks.
                        file.Close();
                        reader.Close();
                        return Load(filename, true);
                    }
                }
                var convl = reader.XmlDeserialize<CNNArgs[]>();
                var network = new ConvolutionalNetwork(matsize, vecsize, depth, labels, convl);
                for (int i = 0; i < network.ConvolutionalLayers.Length; i++)
                    network.ConvolutionalLayers[i].Deserialize(reader);
                network.OutputLayer.Deserialize(reader);
                reader.ReadEndElement();
                file.Close();
                return network;
	        }
	    }
	}
}
