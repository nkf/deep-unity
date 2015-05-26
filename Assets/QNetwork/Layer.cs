using System;
using System.Xml;

namespace QNetwork {
	public abstract class Layer<T> : Unit<T, T> {
        public Layer<T> Prev { get; set; }

        public abstract int Size();

        public abstract T Compute(T input);

        public abstract T Output();

        public abstract V Accept<V>(Trainer<V> t, V state);

        public virtual void Serialize(XmlWriter writer) {
            throw new InvalidOperationException();
        }

        public virtual void Deserialize(XmlReader reader) {
            throw new InvalidOperationException();
        }
	}

    public abstract class TransformationLayer<T, U> : Layer<U>, Unit<T, U> {
        public new Layer<T> Prev { get; set; }

        public abstract U Compute(T input);

        public sealed override U Compute(U input) {
            throw new NotSupportedException();
        }
    }
}
