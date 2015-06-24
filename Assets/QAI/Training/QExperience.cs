using System;
using System.Collections;
using System.Collections.Generic;

namespace QAI.Training {
    [Serializable]
    public class QExperience : IEnumerable<SARS> {
        private readonly List<SARS> _data = new List<SARS>();
        private readonly Random _rng = new Random();

        public void Store(SARS sars, int maxSize) {
            /*Store(sars);
            if (_data.Count > maxSize)
                _data.RemoveAt(0); // TODO: Expensive on List.*/
            if(_data.Count > maxSize)
                _data[_rng.Next(_data.Count)] = sars;
			else
				_data.Add(sars);
        }

        public void Store(SARS sars) {
            _data.Add(sars);
        }

        public void Add(SARS sars) {
            Store(sars);
        }

        public int Count { get { return _data.Count; } }

        public SARS this[int i] {
            get { return _data[i]; }
            set { _data[i] = value; }
        }

        public IEnumerator<SARS> GetEnumerator() {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
