using System;
using System.Collections.Generic;
using System.Linq;

namespace QNetwork {
	public class Layer : Unit<IEnumerable<float>, Layer, Neuron> {
        public int Size { get { return units.Count; } }

        private List<Neuron> units;

        public Layer(int size, ActivationFunction type) {
            units = new List<Neuron>(size);
            for (int i = 0; i < size; i++)
                units.Add(new Neuron(type));
        }

        public Layer(int size, ActivationFunction type, IEnumerable<float> biases) {
            units = new List<Neuron>(size);
            for (int i = 0; i < size; i++)
                units.Add(new Neuron(type));
            biases.ForEach((b, i) => units[i].Bias = b);
        }

        public Layer(IEnumerable<Neuron> units) {
            this.units = units.ToList();
        }

        public Layer(IEnumerable<Neuron> units, IEnumerable<float> biases) {
            this.units = units.ToList();
            biases.ForEach((b, i) => this.units[i].Bias = b);
        }

        public void Accept(IEnumerable<float> input) {
            input.ForEach((x, i) => units[i].Accept(x));
        }

        public void Activate() {
            units.ForEach(u => u.Activate());
        }

        public IEnumerable<float> Output() {
            return units.Select(u => u.Output());
        }

        public void AddConnection(Layer l, IEnumerable<Weight> weights) {
            var w = new Queue<Weight>(weights);
            this.ForEach(u => l.ForEach(ul => u.AddConnection(ul, w.Dequeue().Singleton())));
        }

        public void FinalizeStructure() {
            this.ForEach(u => u.FinalizeStructure());
        }

        public IEnumerator<Neuron> GetEnumerator() {
            return units.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return units.GetEnumerator();
        }
	}
}
