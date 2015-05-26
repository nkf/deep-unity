using MathNet.Numerics.LinearAlgebra;

namespace QNetwork.MLP {
    public class MultiLayerPerceptron : Unit<Vector<float>, Vector<float>> {
        private readonly InputLayer _input;
        private readonly DenseLayer[] _hidden;
        private readonly DenseLayer _output;

        public MultiLayerPerceptron(int insize, params int[] layers) {
            _input = new InputLayer(insize);
            _hidden = new DenseLayer[layers.Length - 1];
            _hidden[0] = new DenseLayer(layers[0], _input, Functions.Sigmoid);
            for (int i = 1; i < _hidden.Length; i++)
                _hidden[i] = new DenseLayer(layers[i], _hidden[i - 1], Functions.Sigmoid);
            _output = new DenseLayer(layers[layers.Length - 1], _hidden[_hidden.Length - 1], Functions.Sigmoid);
        }

        public int Size() {
            return _hidden.Length + 2;
        }

        public Vector<float> Compute(Vector<float> input) {
            return _output.Compute(_hidden.ForwardPropagation(_input.Compute(input)));
        }

        public Vector<float> Output() {
            return _output.Output();
        }

        public T Accept<T>(Trainer<T> t, T state) {
            return _input.Accept(t, _hidden.ApplyTrainer(t, _output.Accept(t, state)));
        }
    }
}
