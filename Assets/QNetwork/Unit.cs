using System.Collections.Generic;
using System.Linq;
using QNetwork.CNN;
using QNetwork.MLP;

namespace QNetwork {
    public interface Unit<T, U> {
        int Size();
        U Compute(T input);
        U Output();
        V Accept<V>(Trainer<V> t, V state);
    }

    public interface Trainer<V> {
        // Visitor pattern.
        V Visit(InputLayer unit, V state);
        V Visit(DenseLayer unit, V state);
        V Visit(SpatialLayer unit, V state);
        V Visit(FlattenLayer unit, V state);
        V Visit(ConvolutionalLayer unit, V state);
        V Visit(MaxPoolLayer unit, V state);
        V Visit(MeanPoolLayer unit, V state);
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
