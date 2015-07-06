using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace QNetwork {
    public struct ActivationFunction<T> {
        public delegate void ValueMapping(T xs, T ys);
        public ValueMapping Apply, Derivatives;
    }

    public static class Functions {
        public static float WeightInitStdDev<T>(int fan_in_out, ActivationFunction<T> func) {
            float interval = (float)Math.Sqrt(6.0 / fan_in_out);
            float stddev = interval / (float)Math.Sqrt(3);
            return stddev;
        }

        public static float BiasInitValue<T>(ActivationFunction<T> func) {
            return func.Apply.Equals(Rectifier.Apply) || func.Apply.Equals(Rectifier2D.Apply) ? 1f : 0f;
        }

        public static ActivationFunction<Vector<float>> Identity = new ActivationFunction<Vector<float>> {
            Apply = (xs, ys) => xs.CopyTo(ys),
            Derivatives = (ys, ds) => ds.MapInplace(d => 1f)
        };

        public static ActivationFunction<Matrix<float>> Identity2D = new ActivationFunction<Matrix<float>> {
            Apply = (xs, ys) => xs.CopyTo(ys),
            Derivatives = (ys, ds) => ds.MapInplace(d => 1f)
        };

        public static ActivationFunction<Vector<float>> Rectifier = new ActivationFunction<Vector<float>> {
            Apply = (xs, ys) => xs.Map(x => Math.Max(0, x), ys),
            Derivatives = (ys, ds) => ys.Map(y => y > 0 ? 1f : 0f, ds)
        };

        public static ActivationFunction<Matrix<float>> Rectifier2D = new ActivationFunction<Matrix<float>> {
            Apply = (xs, ys) => xs.Map(x => Math.Max(0, x), ys),
            Derivatives = (ys, ds) => ys.Map(y => y > 0 ? 1f : 0f, ds)
        };

        public static ActivationFunction<Vector<float>> Sigmoid = new ActivationFunction<Vector<float>> {
            Apply = (xs, ys) => xs.Map(x => 1f / (1f + (float)Math.Exp(-x)), ys, Zeros.Include),
            Derivatives = (ys, ds) => ys.Map(y => y * (1f - y), ds)
        };

        public static ActivationFunction<Vector<float>> Tanh = new ActivationFunction<Vector<float>> {
            Apply = (xs, ys) => xs.Map(x => (float)Math.Tanh(x), ys),
            Derivatives = (ys, ds) => ys.Map(y => 1f - y * y, ds, Zeros.Include)
        };

        public static ActivationFunction<Matrix<float>> Tanh2D = new ActivationFunction<Matrix<float>> {
            Apply = (xs, ys) => xs.Map(x => (float)Math.Tanh(x), ys),
            Derivatives = (ys, ds) => ys.Map(y => 1f - y * y, ds, Zeros.Include)
        };

        public static ActivationFunction<Vector<float>> Softmax = new ActivationFunction<Vector<float>> {
            Apply = (xs, ys) => {
                var xmax = xs.Max();
                xs.Map(x => (float)Math.Exp(x - xmax), ys, Zeros.Include);
                var sum = ys.Sum();
                ys.Map(e => e / sum, ys);
            },
            // NOTE: With Softmax in the output layer, the error term reduces to an equation with no derivative. Therefore it is 1 here.
            Derivatives = (ys, ds) => ds.MapInplace(d => 1f)
        };
    }
}
