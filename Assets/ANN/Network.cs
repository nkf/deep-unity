using System;
using System.Collections.Generic;
using System.Linq;

namespace QNetwork {
	public abstract class Network : Unit<IEnumerable<float>, Network, Layer> {
        protected List<Layer> layers;
        
        public void Accept(IEnumerable<float> input) {
            layers.First().Accept(input);
        }

        public void Activate() {
            layers.ForEach(l => l.Activate());
        }

        public IEnumerable<float> Output() {
            return layers.Last().Output();
        }

        public IEnumerable<float> Compute(IEnumerable<float> input) {
            Accept(input);
            Activate();
            return Output();
        }

        public void AddConnection(Network net, IEnumerable<Weight> weights) {
            layers.Last().AddConnection(net.First(), weights);
        }

        public void FinalizeStructure() {
            this.ForEach(u => u.FinalizeStructure());
        }

        public IEnumerator<Layer> GetEnumerator() {
            return layers.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return layers.GetEnumerator();
        }
	}
}
