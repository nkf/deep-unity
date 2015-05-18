using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using QNetwork.Training;

namespace QNetwork.MLP {
	public class InputLayer : Layer {
        private int size;
        private Vector<float> buffer;

        public InputLayer(int size) {
            this.size = size;
        }

        public override int Size() {
            return size;
        }

        public override Vector<float> Compute(Vector<float> input) {
            return buffer = input;
        }

        public override Vector<float> Output() {
            return buffer;
        }

        public override T Accept<T>(Trainer<T> t, T state) {
            return t.Visit(this, state);
        }
	}
}
