using System.Collections.Generic;
using System.Linq;

namespace QNetwork {
    public interface Unit<T, U> {
        int Size();
        U Compute(T input);
        U Output();
        V Accept<V>(Trainer<V> t, V state);
    }

    public interface Trainer<V> {
        // Visitor pattern.
        V Visit(QNetwork.MLP.InputLayer unit, V state);
        V Visit(QNetwork.MLP.DenseLayer unit, V state);
        V Visit(QNetwork.CNN.SpatialLayer unit, V state);
        V Visit(QNetwork.CNN.FlattenLayer unit, V state);
        V Visit(QNetwork.CNN.ConvolutionalLayer unit, V state);
        V Visit(QNetwork.CNN.MaxPoolLayer unit, V state);
        V Visit(QNetwork.CNN.MeanPoolLayer unit, V state);
        V Visit(QNetwork.Experimental.TreeLayer unit, V state);
    }

    public static class UnitTraversal {

        public static T ForwardPropagation<T>(this IEnumerable<Unit<T, T>> source, T input) {
                return source.Aggregate(input, (xs, unit) => unit.Compute(xs));
        }

        public static V ApplyTrainer<T, V>(this IEnumerable<Unit<T, T>> source, Trainer<V> t, V state) {
            foreach (var unit in source.Reverse()) {
                state = unit.Accept(t, state);
            }
            return state;
            //return source.Reverse().Aggregate(state, (st, unit) => unit.Accept(t, st));
        }
    }
}
