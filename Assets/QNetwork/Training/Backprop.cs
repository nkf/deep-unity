using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using QNetwork.CNN;
using QNetwork.MLP;

namespace QNetwork.Training {
    public struct BackpropState {
        public int DenseLayerIndex;
        public int SpatialLayerIndex;
    }

	public class Backprop : Trainer<BackpropState> {
        public float LearningRate { get; set; }
        public float Momentum { get; set; }
        //public Matrix<float> Features { get; set; }
        //public Matrix<float>[][] Features2D { get; set; }
        //public Matrix<float> Labels { get; set; }

        // Structures for handling flat data.
        internal List<Vector<float>> Error { get; set; }
        internal List<Matrix<float>> Deltas { get; set; }
        internal List<Vector<float>> VBuffer { get; set; }
        internal List<Matrix<float>> MBuffer { get; set; }
        // Structures for handling spatial data.
        internal List<Matrix<float>[]> Error2D { get; set; }
        internal List<Matrix<float>[][]> Deltas2D { get; set; }
        internal List<Matrix<float>> Buffer2D { get; set; }
        internal List<Matrix<float>> EBuffer2D { get; set; }
        internal List<Matrix<float>> Ones { get; set; }

        private readonly ConvolutionalNetwork _net;

        public Backprop(ConvolutionalNetwork network, float lrate, float momentum) {
            LearningRate = lrate;
            Momentum = momentum;
            _net = network;
            Error = new List<Vector<float>>();
            Deltas = new List<Matrix<float>>();
            VBuffer = new List<Vector<float>>();
            MBuffer = new List<Matrix<float>>();
            Error2D = new List<Matrix<float>[]>();
            Deltas2D = new List<Matrix<float>[][]>();
            Buffer2D = new List<Matrix<float>>();
            EBuffer2D = new List<Matrix<float>>();
            Ones = new List<Matrix<float>>();
            network.Accept(new BackpropStateBuilder(this), new BackpropState());
        }

        public void SGD(Matrix<float>[] features, Vector<float> labels) {
            _net.Compute(features);
            labels.CopyTo(Error[0]);
            Error[0].Subtract(_net.Output(), Error[0]);
            _net.Accept(this, new BackpropState());
        }

        public BackpropState Visit(InputLayer unit, BackpropState st) {
            return st;
        }

        public BackpropState Visit(DenseLayer unit, BackpropState st) {
            int i = st.DenseLayerIndex;
            // Multiply incoming error term with derivative of this layer's activation function.
            unit.Activation.Derivatives(unit.Output(), VBuffer[i]);
            Error[i].PointwiseMultiply(VBuffer[i], Error[i]);
            // Calculate outgoing error term (first factor of next layer's error) based on weights and errors in this layer.
            if (i + 1 < Error.Count)
                unit.Weights.TransposeThisAndMultiply(Error[i], Error[i + 1]);
            // Calculate delta weights (applying momentum).
            Error[i].OuterProduct(unit.Prev.Output(), MBuffer[i]);
            MBuffer[i].Multiply(LearningRate, MBuffer[i]);
            Deltas[i].Multiply(Momentum, Deltas[i]);
            Deltas[i].Add(MBuffer[i], Deltas[i]);
            // Adjust weights and biases.
            unit.Weights.Add(Deltas[i], unit.Weights);
            Error[i].Multiply(LearningRate, VBuffer[i]);
            unit.Biases.Add(VBuffer[i], unit.Biases);
            st.DenseLayerIndex++;
            return st;
        }

        public BackpropState Visit(SpatialLayer unit, BackpropState st) {
            return st;
        }

        public BackpropState Visit(FlattenLayer unit, BackpropState st) {
            // Unflatten error.
            for (int j = 0; j < unit.Z; j++)
                for (int m = 0; m < unit.X; m++)
                    Error[Error.Count - 1].CopySubVectorTo(Error2D[0][j].Row(m), j * unit.X * unit.Y + m * unit.Y, 0, unit.Y);
            return st;
        }

        public BackpropState Visit(ConvolutionalLayer unit, BackpropState st) {
            int k = st.SpatialLayerIndex;
            int fsize = unit.Weights[0][0].RowCount;
            var output = unit.Prev.Output();
            // Clear next layer's error.
            if (k + 1 < Error2D.Count)
                for (int i = 0; i < unit.Prev.ChannelCount; i++)
                    Error2D[k + 1][i].Clear();
            for (int j = 0; j < unit.ChannelCount; j++) {
                // Multiply incoming error term with derivative of this layer's activation function.
                unit.Activation.Derivatives(unit.Output()[j], EBuffer2D[k]);
                Error2D[k][j].PointwiseMultiply(EBuffer2D[k], Error2D[k][j]);
                // Propagate error to next layer.
                if (k + 1 < Error2D.Count) {
                    for (int i = 0; i < unit.Prev.ChannelCount; i++) {
                        for (int m = 0; m < EBuffer2D[k].RowCount; m += unit.Stride)
                            for (int n = 0; n < EBuffer2D[k].ColumnCount; n += unit.Stride) {
                                unit.Weights[i][j].Multiply(Error2D[k][j].At(m, n), Buffer2D[k]);
                                var subm = Error2D[k + 1][i].SubMatrix(m, fsize, n, fsize);
                                subm.Add(Buffer2D[k], subm);
                            }
                    }
                }
                // Adjust weights and biases.
                for (int i = 0; i < unit.Prev.ChannelCount; i++) {
                    Deltas2D[k][i][j].Multiply(Momentum, Deltas2D[k][i][j]);
                    for (int m = 0; m < Error2D[k][j].RowCount; m += unit.Stride)
                        for (int n = 0; n < Error2D[k][j].ColumnCount; n += unit.Stride) {
                            output[i].SubMatrix(m, fsize, n, fsize).Multiply(Error2D[k][j].At(m, n) * LearningRate, Buffer2D[k]);
                            Deltas2D[k][i][j].Add(Buffer2D[k], Deltas2D[k][i][j]);
                        }
                    unit.Weights[i][j].Add(Deltas2D[k][i][j], unit.Weights[i][j]);
                }
                unit.Biases.At(j, unit.Biases.At(j) + Error2D[k][j].RowSums().Sum() * LearningRate);
            }
            st.SpatialLayerIndex++;
            return st;
        }

        public BackpropState Visit(MaxPoolLayer unit, BackpropState st) {
            throw new NotImplementedException();
        }

        public BackpropState Visit(MeanPoolLayer unit, BackpropState st) {
            int i = st.SpatialLayerIndex;
            // Upsample the error by first taking the Kronecker product of a matrix of 1's the same size as the pooling region.
            // Then divide by the total size of the pooling region, thus distributing the error by mean.
            for (int j = 0; j < unit.ChannelCount; j++) {
                Error2D[i][j].KroneckerProduct(Ones[i], Error2D[i + 1][j]);
                Error2D[i + 1][j].Divide(unit.PoolSize * unit.PoolSize, Error2D[i + 1][j]);
            }
            st.SpatialLayerIndex++;
            return st;
        }
	}

    public class BackpropStateBuilder : Trainer<BackpropState> {
        private readonly Backprop t;

        public BackpropStateBuilder(Backprop trainer) {
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
            return st;
        }

        public BackpropState Visit(MaxPoolLayer unit, BackpropState st) {
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
    }
}
