using System;
using System.Collections.Generic;
using System.Linq;
using Math = UnityEngine.Mathf;

namespace QNetwork.MLP {
	public class MultiLayerPerceptron : Network {
        public MultiLayerPerceptron(int[] composition, bool[] biases) {
            layers = new List<Layer>(composition.Length);
            var bs = biases[0] ? Utils.RandomList(composition[0], -0.1f, 0.1f) : Enumerable.Empty<float>();
            var prev = new Layer(composition[0], Utils.Identity, bs);
            layers.Add(prev);
            int size;
            for (int i = 1; i < composition.Length; i++) {
                size = composition[i];
                bs = biases[i] ? Utils.RandomList(composition[i], -0.1f, 0.1f) : Enumerable.Empty<float>();
                var ws = Utils.RandomList(size * prev.Size, -1f / Math.Sqrt(prev.Size), 1f / Math.Sqrt(prev.Size));
                prev.AddConnection(prev = new Layer(size, Utils.Sigmoid, bs), ws.Select(w => new SimpleWeight(w)).Cast<Weight>());
                layers.Add(prev);
            }
            FinalizeStructure();
        }
	}
}
