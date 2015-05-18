using System;
using System.Collections.Generic;
using System.Linq;
using Math = UnityEngine.Mathf;
using Random = System.Random;

namespace QNetwork {
	public static class Utils {
        private static Random rng = new Random();

        public static ActivationFunction Identity = new ActivationFunction {
            fx = x => x,
            dy = y => 1f
        };

        public static ActivationFunction Rectifier = new ActivationFunction {
            fx = x => Math.Max(0, x),
            dy = y => y > 0 ? 1f : 0f
        };

        public static ActivationFunction Sigmoid = new ActivationFunction {
            fx = x => 1f / (1f + Math.Exp(-x)),
            dy = y => y * (1f - y)
        };

        public static ActivationFunction Tanh = new ActivationFunction {
            fx = x => (float)System.Math.Tanh(x),
            dy = y => 1f - y * y
        };

        public static IEnumerable<float> RandomList(int length, float min, float max) {
            for (int i = 0; i < length; i++)
                yield return (float)rng.NextDouble() * (max - min) + min;
        }
	}

    public struct ActivationFunction {
        public delegate float Continuous(float x);
        public Continuous fx, dy;
    }
}
