using QNetwork.CNN;
using QNetwork.Experimental;
using QNetwork.MLP;
using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.Training {
    public class BackpropStateBuilder<T> : Trainer<BackpropState> {
        private readonly Backprop<T> t;

        public BackpropStateBuilder(Backprop<T> trainer) {
            t = trainer;
        }

        public BackpropState Visit(InputLayer unit, BackpropState st) {
            return st;
        }

        public BackpropState Visit(DenseLayer unit, BackpropState st) {
            t.Error.Add(Vector<float>.Build.Dense(unit.Size()));
            t.VBuffer.Add(Vector<float>.Build.Dense(unit.Size()));
            t.Deltas.Add(Matrix<float>.Build.Dense(unit.Size(), unit.Prev.Size()));
            t.MBuffer.Add(Matrix<float>.Build.Dense(unit.Size(), unit.Prev.Size()));
            return st;
        }

        public BackpropState Visit(SpatialLayer unit, BackpropState st) {
            return st;
        }

        public BackpropState Visit(FlattenLayer unit, BackpropState st) {
            t.Error.Add(Vector<float>.Build.Dense(unit.Size()));
            return st;
        }

        public BackpropState Visit(ConvolutionalLayer unit, BackpropState st) {
            var err2d = new Matrix<float>[unit.ChannelCount];
            for (int i = 0; i < unit.ChannelCount; i++)
                err2d[i] = Matrix<float>.Build.Dense(unit.SideLength, unit.SideLength);
            t.Error2D.Add(err2d);
            t.EBuffer2D.Add(Matrix<float>.Build.Dense(unit.SideLength, unit.SideLength));
            int fsize = unit.Weights[0][0].RowCount;
            t.Buffer2D.Add(Matrix<float>.Build.Dense(fsize, fsize));
            var d2d = new Matrix<float>[unit.Prev.ChannelCount][];
            for (int i = 0; i < d2d.Length; i++) {
                d2d[i] = new Matrix<float>[unit.ChannelCount];
                for (int j = 0; j < unit.ChannelCount; j++)
                    d2d[i][j] = Matrix<float>.Build.Dense(fsize, fsize);
            }
            t.Deltas2D.Add(d2d);
            t.Ones.Add(null);
            return st;
        }

        public BackpropState Visit(MaxPoolLayer unit, BackpropState st) {
            var err2d = new Matrix<float>[unit.ChannelCount];
            for (int i = 0; i < unit.ChannelCount; i++)
                err2d[i] = Matrix<float>.Build.Dense(unit.SideLength, unit.SideLength);
            t.Error2D.Add(err2d);
            t.Ones.Add(Matrix<float>.Build.Dense(unit.PoolSize, unit.PoolSize, 1f));
            t.EBuffer2D.Add(null);
            t.Buffer2D.Add(null);
            t.Deltas2D.Add(null);
            return st;
        }

        public BackpropState Visit(MeanPoolLayer unit, BackpropState st) {
            var err2d = new Matrix<float>[unit.ChannelCount];
            for (int i = 0; i < unit.ChannelCount; i++)
                err2d[i] = Matrix<float>.Build.Dense(unit.SideLength, unit.SideLength);
            t.Error2D.Add(err2d);
            t.Ones.Add(Matrix<float>.Build.Dense(unit.PoolSize, unit.PoolSize, 1f));
            t.EBuffer2D.Add(null);
            t.Buffer2D.Add(null);
            t.Deltas2D.Add(null);
            return st;
        }

        public BackpropState Visit(TreeLayer unit, BackpropState st) {
            // TODO
            return st;
        }
    }
}
