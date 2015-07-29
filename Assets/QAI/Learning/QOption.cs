using QNetwork.CNN;

namespace QAI.Learning {
    public class QOption {
        public bool Discretize = true;
        public int TrainingInterval = 20;
        public int TrainingCycle = 10;
        public int BatchSize = 2000;
        public int MaxPoolSize = 2000;
        public float Discount = 0.95f;
        public float EpsilonStart = 0.5f;
        public float EpsilonEnd = 0.1f;
        public float LearningRate = 0.005f;
        public CNNArgs[] NetworkArgs = { new CNNArgs {FilterSize = 4, FilterCount = 1, PoolLayerSize = 2, Stride = 2} };
    }
}
