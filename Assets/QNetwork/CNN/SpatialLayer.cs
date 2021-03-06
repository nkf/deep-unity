﻿using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.CNN {
	public class SpatialLayer : Layer<Matrix<float>[]> {
        protected Matrix<float>[] _values;
        public new SpatialLayer Prev { get; protected set; }
        public int SideLength { get; private set; }
        public int ChannelCount { get; private set; }

        public SpatialLayer(int dimension, int channels) {
            SideLength = dimension;
            ChannelCount = channels;
        }

        public override int Size() {
            return SideLength * SideLength * ChannelCount;
        }

        public override Matrix<float>[] Compute(Matrix<float>[] input) {
            return _values = input;
        }

        public override Matrix<float>[] Output() {
            return _values;
        }
    }
}
