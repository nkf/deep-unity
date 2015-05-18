using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace QNetwork {
	public abstract class Layer : Unit<Vector<float>> {
        public Layer Prev { get; set; }
        public Layer Next { get; set; }

        public abstract int Size();

        public abstract Vector<float> Compute(Vector<float> input);

        public abstract Vector<float> Output();

        public abstract T Accept<T>(Trainer<T> t, T state);
	}
}
