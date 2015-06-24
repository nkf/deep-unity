namespace QNetwork {
    public struct TargetIndexPair {
        public readonly float Target;
        public readonly int Index;
        public TargetIndexPair(float t, int i) : this() {
            Target = t;
            Index = i;
        }
    }

    public interface Network<T, U> : Unit<T, U> {
        void InitializeTraining(BackpropParams par);
        void SGD(T features, U labels);
        void SGD(T features, TargetIndexPair p);
        void Save(string filename);
    }
}
