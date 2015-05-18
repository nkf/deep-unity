using System;
using System.Collections.Generic;
using System.Linq;

namespace QNetwork {
	public interface Unit<T, U, V> : IEnumerable<V> where U : Unit<T, U, V> {
        void Accept(T input);
        void Activate();
        T Output();
        void AddConnection(U unit, IEnumerable<Weight> weights);
        void FinalizeStructure();
	}
}
