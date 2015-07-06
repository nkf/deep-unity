using System.Collections.Generic;
using System.Linq;

namespace QNetwork {
    public struct BackpropParams {
        public float LearningRate;
        public float Momentum;
        public float Decay;
    }

	public interface Backprop<T, U> {
        U Visit(T incoming, BackpropParams par);
	}

    public static class BackpropTraversal {
        public static T BackPropagation<T>(this IEnumerable<Backprop<T, T>> source, T feedback, BackpropParams par) {
            return source.Aggregate(feedback, (xs, bp) => bp.Visit(xs, par));
        }
    }
}
