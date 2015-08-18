using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using QAI.Agent;

namespace QAI.Utility {
    public class FrameStacker {
        public int Count { get; private set; }
        private readonly List<QState> _prev;
        public FrameStacker(int count) {
            Count = count;
            _prev = new List<QState>();
        }

        public QState GetStack() {
            //Stack spatial
            var spatial = _prev.Select(s => s.Features.Spatial[0]).ToArray();
            //Stack Linear
            var n = _prev[0].Features.Linear.Count;
            var linear = Vector<float>.Build.Dense(n * Count);
            for (var i = 0; i < Count; i++) {
                _prev[i].Features.Linear.CopySubVectorTo(linear, 0, i*n, n);
            }

            return new QState(spatial, linear, _prev[0].Reward, _prev[0].IsTerminal);
        }

        public void Add(QState state) {
            if (_prev.Count == 0) {
                for (var i = 0; i < Count; i++) _prev.Add(state);
            } else {
                _prev.RemoveAt(Count-1);
                _prev.Insert(0, state);
            }
        }
        
    }
}
