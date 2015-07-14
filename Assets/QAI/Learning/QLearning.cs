using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using QAI.Agent;
using QAI.Training;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace QAI.Learning {
    public abstract class QLearning {

        private const double TIE_BREAK = 1e-9;

        public const float EpisilonStart = 0.5f;
        public const float EpisilonEnd = 0.1f;
        public readonly Param Epsilon = t => EpisilonStart + ((EpisilonEnd - EpisilonStart) / QAIManager.NumIterations()) * t;
        public const float Discount = 0.95f;

        public const bool PrioritySweeping = false;

        //Number of timesteps inbetween training sessions
        public const int TrainInterval = 8;
        //Number of batches being trained each session
        public const int TraningCycles = 5;
        //Number of sars being trained in each training cycle
        public const int BatchSize = PrioritySweeping ? 5 : 100;
        //Maximum number of sars being kept
        public const int MaxStoreSize = PrioritySweeping ? 30 : 100;
        //TODO: write what this is
        public const int PredecessorCap = 6;
        //TODO: write what this is
        public const float PriorityThreshold = 0.005f;

        public ReadOnlyCollection<QAction> Actions { get; private set; }
        public int Iteration { get; set; }

        public delegate float Param(float t);
        public delegate float ActionValueFunction(QAction a);

        public abstract void Initialize(int gridsize, int vectorsize);

        public abstract void SaveModel();
        public abstract void LoadModel();
        public abstract void RemakeModel(QState exampleState);
        public abstract void TrainModel(List<SARS> batch);
        public abstract bool ModelReady();
        public abstract QAI.Visualizer.NetworkVisualizer CreateVisualizer();

        public abstract ActionValueFunction Q(QState s);
        public abstract float QMax(QState s);

        private int _trainingCounter;
        private bool _isFirstTurn = true;
        private QState _prevState;
        private QAction _prevAction;

        protected readonly QExperience _qexp = new QExperience();
        protected readonly Dictionary<QState, List<SARS>> _preds = new Dictionary<QState, List<SARS>>(1000);
        protected readonly C5.IntervalHeap<SARS> _pq = new C5.IntervalHeap<SARS>(200, new SARSPrioritizer());

        public void Reset(QAgent agent) {
            Actions = agent.GetQActions().AsReadOnly();
        }

        public Action GetLearningAction(QState state) {
            if (!ModelReady()) {
                Iteration = 1;
                Initialize(state.GridSize, state.VectorSize);
            }
            if (!_isFirstTurn) {
                if (state.IsTerminal) {
                    StoreSARS(new SARS(_prevState, _prevAction, state));
                    _isFirstTurn = true;
                    return null;
                }
                if (state.Equals(_prevState)) {
                    return _prevAction.Action;
                }
                StoreSARS(new SARS(_prevState, _prevAction, state));
            }
            var a = EpsilonGreedy(Epsilon(Iteration), state);
            _prevAction = a;
            _prevState = state;
            _isFirstTurn = false;
            _trainingCounter++;
            if (_trainingCounter >= TrainInterval) {
                _trainingCounter = 0;
                var ts = Time.timeScale;
                Time.timeScale = 0;
                QAIManager.RunCoroutine(PrioritySweeping ? RunPriotizedTraining(ts) : RunTraining(ts));
            }
            return a.Action;
        }

        private IEnumerator RunTraining(float timescale) {
            var batches = SampleBatch(BatchSize).Partition(TraningCycles);
            foreach (var batch in batches) {
                TrainModel(batch);
                yield return new WaitForEndOfFrame();
            }
            Time.timeScale = timescale;
        }

        private IEnumerator RunPriotizedTraining(float timescale) {
            for (int i = 0; i < TraningCycles; i++) {
                PrioritizedSweeping();
                yield return new WaitForEndOfFrame();
            }
            Time.timeScale = timescale;
        }

        public QAction GreedyPolicy(QState state) {
            var q = Q(state);
            return ValidActions().Select(a => new { v = q(a) + TIE_BREAK * Random.value, a }).OrderByDescending(va => va.v).First().a;
        }

        public QAction ProbabilisticPolicy(QState state) {
            var q = Q(state);
            var vas = ValidActions();
            var es = vas.Select(v => Math.Exp(q(v)));
            double sum = es.Sum();
            var aux = es.Select((e, i) => new { p = e / sum, a = vas[i] }).OrderBy(x => x.p);
            double rng = Random.value;
            foreach (var x in aux)
                if (rng < x.p)
                    return x.a;
                else
                    rng -= x.p;
            return aux.Last().a;
        }

        public QAction EpsilonGreedy(double eps, QState state) {
            return EpsilonPolicy(eps, GreedyPolicy, state);
        }

        public QAction EpsilonPropabalistic(double eps, QState state) {
            return EpsilonPolicy(eps, ProbabilisticPolicy, state);
        }

        private QAction EpsilonPolicy(double eps, Func<QState, QAction> policy, QState state) {
            if(Random.value < eps) {
                var valid = ValidActions();
                return valid[Random.Range(0, valid.Count)];
            }
            return policy(state);
        }

        /// <summary>
        /// Always returns a non-empty list. Contains the NullAction if no valid actions are available.
        /// </summary>
        /// <returns>List of valid actions.</returns>
        protected IList<QAction> ValidActions() {
            var actions = Actions.Where(a => a.IsValid()).ToList();
            if (actions.Count == 0)
                actions.Add(QAction.NullAction);
            return actions;
        }

        protected void LoadExperienceDatabase() {
            var exps = QStory.LoadAll("QData/Story")
                .Where(qs => qs.ScenePath == EditorApplication.currentScene)
                .SelectMany(qs => qs.ImitationExperiences.Select(qi => qi.Experience))
                .SelectMany(e => e).ToList();
            Debug.Log("Loading " + exps.Count + " imitation experiences");
            foreach (var sars in exps)
                StoreSARS(sars);
        }

        protected List<SARS> SampleBatch(int size) {
            return _qexp.Shuffle().Take(size).ToList();
        }

        protected List<SARS> FullBatch() {
            return _qexp.Shuffle().ToList();
        }

        protected void StoreSARS(SARS sars) {
            if (PrioritySweeping) {
                PutPredecessor(sars);
                EnqueueSARS(sars);
                while (_pq.Count > MaxStoreSize)
                    _pq.DeleteMin();
            } else {
                _qexp.Store(sars, MaxStoreSize);
            }
        }

        private void PrioritizedSweeping() {
            int N = Mathf.Min(BatchSize, _pq.Count);
            /*if(_pq.Count > 0)
                Debug.Log("LEARNING " + _pq.Count);*/
            var batch = Enumerable.Range(0, N).Select(i => _pq.DeleteMax()).ToList();
            TrainModel(batch);
            foreach (var sars in batch)
                if (_preds.ContainsKey(sars.State))
                    foreach (var pred in _preds[sars.State].Shuffle().Take(PredecessorCap))
                        EnqueueSARS(pred);
        }

        private void EnqueueSARS(SARS s) {
            var q = Q(s.State)(s.Action);
            if (!s.NextState.IsTerminal) {
                var a0max = QMax(s.NextState);
                s.Priority = Mathf.Abs(s.Reward + Discount * a0max - q);
            } else {
                s.Priority = Mathf.Abs(s.Reward - q);
            }
            if (s.Priority > PriorityThreshold)
                _pq.Add(s);
        }

        private void PutPredecessor(SARS sars) {
            if (!_preds.ContainsKey(sars.NextState))
                _preds[sars.NextState] = new List<SARS>();
            if (sars.NextState.Equals(sars.State)) {
                Debug.Log("SELF REFERENCE. IS TERM : " + sars.State.IsTerminal + " - " + sars.NextState.IsTerminal
                           + " ;r " + sars.State.Reward + " - " + sars.NextState.Reward
                           + " ;h " + sars.State.GetHashCode() + " - " + sars.NextState.GetHashCode());
                return;
            }
            var p = _preds[sars.NextState];
            if (!p.Contains(sars))
                p.Add(sars);
        }
    }
}
