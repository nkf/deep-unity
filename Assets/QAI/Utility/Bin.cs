using System;
using MathNet.Numerics.LinearAlgebra;

namespace QAI.Utility {
    public class Bin {
        private readonly float[] _intervals;
		private readonly Vector<float> _values;

        public Bin(params float[] intervals) {
            if(intervals.Length < 1) throw new ArgumentException("At least 1 interval values must be declared");
            _intervals = intervals;
			_values = Vector<float>.Build.Dense(_intervals.Length+1, 0);
        }

        public Vector<float> this[float value] { get { return Get(value); } } 

		public Vector<float> Get(float value) {
			_values.Clear();
            for (var i = 0; i < _intervals.Length; i++) {
                if (value < _intervals[i]) {
                    _values[i] = 1;
                    return _values;
                }
            }
//            _values[_values.Length - 1] = 1;
            return _values.Clone ();
        }

    }
}
