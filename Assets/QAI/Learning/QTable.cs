using QAI.Agent;

namespace QAI.Learning {
    public class QTable {
        private readonly SerializableDictionary<QState, SerializableDictionary<QAction, double>> _table;
        private readonly double defr;

        public QTable(double defaultReward) {
            _table = new SerializableDictionary<QState, SerializableDictionary<QAction, double>>();
            defr = defaultReward;
        }

        public void Add(QState s, QAction a, double v) {
            SerializableDictionary<QAction, double> qa;
            if(!_table.ContainsKey(s)) {
                qa = new SerializableDictionary<QAction, double>();
                _table.Add(s, qa);
            } else {
                qa = _table[s];
            }
            qa[a] = v;
        }

        public double Query(QState s, QAction a) {
            if(!_table.ContainsKey(s)) return defr;
            var qa = _table[s];
            if(!qa.ContainsKey(a)) return defr;
            return qa[a];
        }

        public void Load(string path) {
            QData.Load(path, _table);
        }

        public void Save(string path) {
            QData.Save(path, _table);
        }
    }
}
