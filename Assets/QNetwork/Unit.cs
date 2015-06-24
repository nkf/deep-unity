using System.Collections.Generic;
using System.Linq;

namespace QNetwork {
    public interface Unit<T, U> {
        int Size();
        U Compute(T input);
        U Output();
    }

    public static class UnitTraversal {
        public static T ForwardPropagation<T>(this IEnumerable<Unit<T, T>> source, T input) {
                return source.Aggregate(input, (xs, unit) => unit.Compute(xs));
        }
    }
}
