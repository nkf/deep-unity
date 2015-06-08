using System.Xml;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.CNN {
	public class ConvolutionalLayer : SpatialLayer {
        public ActivationFunction<Matrix<float>> Activation;
	    public Vector<float> Biases { get; set; }
	    public Matrix<float>[][] Weights { get; set; }
	    public new SpatialLayer Prev { get; set; }
        public int Stride { get; set; }
        
        private readonly Matrix<float> _cache, _buffer;

	    public ConvolutionalLayer(int fsize, int numf, int stride, SpatialLayer prev, ActivationFunction<Matrix<float>> activation)
            : base((prev.SideLength - fsize) / stride + 1, numf) {
            Activation = activation;
            var vb = Vector<float>.Build;
            var mb = Matrix<float>.Build;
            _values = new Matrix<float>[ChannelCount];
            for (int i = 0; i < _values.Length; i++)
                _values[i] = Matrix<float>.Build.Dense(SideLength, SideLength);
            Biases = vb.Random(ChannelCount, Normal.WithMeanStdDev(0.0, 0.05));
            Weights = new Matrix<float>[prev.ChannelCount][];
            for (int i = 0; i < Weights.Length; i++) {
                Weights[i] = new Matrix<float>[ChannelCount];
                for (int j = 0; j < Weights[i].Length; j++)
                    Weights[i][j] = mb.Random(fsize, fsize, Normal.WithMeanStdDev(0.0, 0.5 / (SideLength * SideLength)));
            }
            Prev = prev;
            Stride = stride;
            _cache = mb.Dense(fsize, fsize);
            if (prev.ChannelCount > 1) // No buffer needed if there is only 1 input channel.
                _buffer = mb.Dense(SideLength, SideLength);
        }

        public override Matrix<float>[] Compute(Matrix<float>[] input) {
            // Input from first channel. No buffer needed (values are overwritten).
            for (int j = 0; j < Weights[0].Length; j++)
                Convolution(input[0], Weights[0][j], _values[j]);
            // Input from remaining channels. Use of buffer in order to accumulate values.
            for (int i = 1; i < Weights.Length; i++) {
                for (int j = 0; j < Weights[i].Length; j++) {
                    Convolution(input[i], Weights[i][j], _buffer);
                    _values[j].Add(_buffer, _values[j]);
                }
            }
            // Add biases and apply activation function.
            for (int i = 0; i < _values.Length; i++) {
                _values[i].Add(Biases.At(i), _values[i]);
                Activation.Apply(_values[i], _values[i]);
            }
            return _values;
        }

        private void Convolution(Matrix<float> source, Matrix<float> filter, Matrix<float> dest) {
            for (int m = 0; m < dest.RowCount; m++)
                for (int n = 0; n < dest.ColumnCount; n++) {
                    source.SubMatrix(m * Stride, filter.RowCount, n * Stride, filter.ColumnCount).PointwiseMultiply(filter, _cache);
                    dest.At(m, n, _cache.RowSums().Sum());
                }
        }

        public override T Accept<T>(Trainer<T> t, T state) {
            return t.Visit(this, state);
        }

        public override void Serialize(XmlWriter writer) {
            writer.WriteStartElement(GetType().Name);

            for (int x = 0; x < Weights.Length; x++) {
                for (int y = 0; y < Weights[x].Length; y++) {
                    writer.XmlSerialize(Weights[x][y].ToColumnArrays());
                }
            }
            writer.XmlSerialize(Biases.ToArray());

            writer.WriteEndElement();
        }

        public override void Deserialize(XmlReader reader) {
            reader.ReadStartElement(GetType().Name);

            var mb = Matrix<float>.Build;
            for (int x = 0; x < Weights.Length; x++) {
                for (int y = 0; y < Weights[x].Length; y++) {
                    var wData = reader.XmlDeserialize<float[][]>();
                    Weights[x][y] = mb.DenseOfColumnArrays(wData);
                }
            }
            Biases = Vector<float>.Build.Dense( reader.XmlDeserialize<float[]>() );

            reader.ReadEndElement();
        }
	}
}
