using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using Math = UnityEngine.Mathf;

namespace QNetwork {
    public struct ActivationFunction<T> {
        public delegate void ValueMapping(T xs, T ys);
        public ValueMapping Apply, Derivatives;
    }

    public static class Functions {
        public static ActivationFunction<Vector<float>> Identity = new ActivationFunction<Vector<float>> {
            Apply = (xs, ys) => xs.CopyTo(ys),
            Derivatives = (ys, ds) => ds.MapInplace(d => 1f)
        };

        public static ActivationFunction<Vector<float>> Rectifier = new ActivationFunction<Vector<float>> {
            Apply = (xs, ys) => xs.Map(x => Math.Max(0, x), ys),
            Derivatives = (ys, ds) => ys.Map(y => y > 0 ? 1f : 0f, ds)
        };

        public static ActivationFunction<Vector<float>> Sigmoid = new ActivationFunction<Vector<float>> {
            Apply = (xs, ys) => xs.Map(x => 1f / (1f + Math.Exp(-x)), ys),
            Derivatives = (ys, ds) => ys.Map(y => y * (1f - y), ds)
        };

        public static ActivationFunction<Vector<float>> Tanh = new ActivationFunction<Vector<float>> {
            Apply = (xs, ys) => xs.Map(x => (float)System.Math.Tanh(x), ys),
            Derivatives = (ys, ds) => ys.Map(y => 1f - y * y, ds)
        };

        public static ActivationFunction<Matrix<float>> Tanh2D = new ActivationFunction<Matrix<float>> {
            Apply = (xs, ys) => xs.Map(x => (float)System.Math.Tanh(x), ys),
            Derivatives = (ys, ds) => ys.Map(y => 1f - y * y, ds)
        };

        public static ActivationFunction<Vector<float>> Softmax = new ActivationFunction<Vector<float>> {
            Apply = (xs, ys) => {
                var xmax = xs.Max();
                xs.Map(x => Math.Exp(x - xmax), ys);
                var sum = ys.Sum();
                ys.Map(e => e / sum, ys);
            },
            // NOTE: With Softmax in the output layer, the error term reduces to an equation with no derivative. Therefore it is 1 here.
            Derivatives = (ys, ds) => ds.MapInplace(d => 1f)
        };
    }
}
