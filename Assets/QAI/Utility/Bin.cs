using System;

public class Bin {
    private readonly float[] _intervals;
    public Bin(params float[] intervals) {
        if(intervals.Length < 1) throw new ArgumentException("At least 1 interval values must be declared");
        _intervals = intervals;
    }

    public double[] this[float value] { get { return Get(value); } } 

    public double[] Get(float value) {
        var state = new double[_intervals.Length+1];
        for (var i = 0; i < _intervals.Length; i++) {
            if (value < _intervals[i]) {
                state[i] = 1;
                return state;
            }
        }
        state[state.Length - 1] = 1;
        return state;
    }

}
