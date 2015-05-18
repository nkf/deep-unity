using System;
using System.Collections.Generic;
using System.Linq;
using QNetwork.CNN;
using QNetwork.MLP;

namespace QNetwork {
    public interface Unit<T> {
        int Size();
        T Compute(T input);
        T Output();
        V Accept<V>(Trainer<V> t, V state);
    }

    public interface Network<T> : Unit<T> {
        IEnumerable<Unit<T>> BottomUp();
        IEnumerable<Unit<T>> TopDown();
    }

    public interface Trainer<V> {
        // Visitor pattern.
        V Visit(InputLayer unit, V state);
        V Visit(DenseLayer unit, V state);
        V Visit(SpatialLayer unit, V state);
        V Visit(ConvolutionalLayer unit, V state);
        V Visit(MaxPoolLayer unit, V state);
        V Visit(MeanPoolLayer unit, V state);
    }

    public static class UnitTraversal {
        public static T ForwardPropagation<T>(this IEnumerable<Unit<T>> source, T input) {
                return source.Aggregate(input, (xs, unit) => unit.Compute(xs));
        }

        public static V ApplyTrainer<T, V>(this IEnumerable<Unit<T>> source, Trainer<V> t, V state) {
                return source.Aggregate(state, (st, unit) => unit.Accept(t, st));
        }
    }
}
