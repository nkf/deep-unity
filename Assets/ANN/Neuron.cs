using System;
using System.Collections.Generic;
using System.Linq;

namespace QNetwork {
    public class Neuron : Unit<float, Neuron, Neuron.Connection> {
        private List<Neuron> ns = new List<Neuron>(); // Neurons this one connects to.
        private List<Weight> ws = new List<Weight>(); // Weights for the above connections.
        private float x = 0f; // Accumulated activation energy.

        private IEnumerable<Connection> Connections { get; set; }
        public ActivationFunction Function { get; private set; }
        public float Value { get; private set; }
        public float Error { get; set; }
        public float? Bias { get; set; }

        public Neuron(ActivationFunction func) {
            Function = func;
        }

        public void Accept(float input) {
            x += input;
        }

        public void Activate() {
            Value = Function.fx(x + (Bias ?? 0f));
            ns.ForEach((n, i) => n.Accept(Value * ws[i].Magnitude()));
            x = 0f;
        }

        public float Output() {
            return Value;
        }

        public void AddConnection(Neuron n, IEnumerable<Weight> weights) {
            foreach (var w in weights) {
                ns.Add(n);
                ws.Add(w);
            }
        }

        public void FinalizeStructure() {
            Connections = Enumerable.Range(0, ns.Count).Select(i => new Connection(this, i)).ToList();
        }

        public IEnumerator<Connection> GetEnumerator() {
            return Connections.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return Connections.GetEnumerator();
        }

        public struct Connection {
            private Neuron n;
            private int i;
            public Neuron Neuron { get { return n.ns[i]; } }
            public float Weight { get { return n.ws[i].Magnitude(); } }
            public float PrevDeltaWeight { get { return n.ws[i].PrevDelta(); } }
            public Connection(Neuron n, int index) {
                this.n = n;
                i = index;
            }
            public void AdjustWeight(float delta, float ext) {
                n.ws[i].Adjust(delta, ext);
            }
        }
	}
}
