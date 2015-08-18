using System;
using MathNet.Numerics.LinearAlgebra;

namespace QAI.Utility {
    public class Bin {
        private readonly float[] _intervals;

        public Bin(params float[] intervals) {
            if(intervals.Length < 1) throw new ArgumentException("At least 1 interval values must be declared");
            _intervals = intervals;
        }

        public int Count { get { return _intervals.Length + 1; } }
        public float[] this[float value] { get { return Get(value); } } 

		public float[] Get(float value) {
            var values = new float[_intervals.Length + 1];
            for (var i = 0; i < _intervals.Length; i++) {
                if (value < _intervals[i]) {
                    values[i] = 1;
                    return values;
                }
            }
            values[values.Length - 1] = 1;
		    return values;
		}

    }
}
