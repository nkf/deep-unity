using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.CNN {
    public class FlattenLayer : TransformationLayer<Matrix<float>[], Vector<float>> {
        private readonly int _size;
        private readonly Vector<float> _values;
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public FlattenLayer(SpatialLayer prev) {
            Prev = prev;
            X = prev.SideLength;
            Y = prev.SideLength;
            Z = prev.ChannelCount;
            _size = prev.SideLength * prev.SideLength * prev.ChannelCount;
            _values = Vector<float>.Build.Dense(_size);
        }

        public override int Size() {
            return _size;
        }

        public override Vector<float> Compute(Matrix<float>[] input) {
            int i = 0;
            for (int c = 0; c < input.Length; c++)
                for (int m = 0; m < input[c].RowCount; m++)
                    for (int n = 0; n < input[c].ColumnCount; n++)
                        _values[i++] = input[c].At(m, n);
            return _values;
        }

        public override Vector<float> Output() {
            return _values;
        }
    }
}
