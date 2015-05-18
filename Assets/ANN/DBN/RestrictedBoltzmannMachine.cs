using System;
using System.Collections.Generic;
using System.Linq;

namespace QNetwork.DBN {
	public class RestrictedBoltzmannMachine : Network {
        public RestrictedBoltzmannMachine(int visible, int hidden) {
            layers = new List<Layer>(2);
            layers[0] = new Layer(visible, Utils.Sigmoid);
            layers[1] = new Layer(hidden, Utils.Sigmoid);
        }
	}
}
