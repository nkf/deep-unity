using System;
using System.Collections.Generic;
using System.Linq;

namespace QNetwork {
    public struct DataPair<T> {
        public IEnumerable<T> Input { get; set; }
        public IEnumerable<T> Targets { get; set; }
        public DataPair(IEnumerable<T> input, IEnumerable<T> targets) {
            Input = input;
            Targets = targets;
        }
    }

    public delegate float LearningRate<T>(T status);
    public delegate bool TerminationCondition<T>(T status);

	public interface Trainer<T, U> {
        void PassOne(T data);
        void PassAll(IEnumerable<T> data);
        void Train(IEnumerable<T> data);
        void Reset();
        void SetLearningRate(LearningRate<U> rate);
        void SetTerminationCondition(TerminationCondition<U> condition);
	}
}
