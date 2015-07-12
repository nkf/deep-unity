using QAI.Agent;
using QAI.Training;
using UnityEngine;

namespace QAI.Utility {
    public abstract class QTester : MonoBehaviour {
        public abstract void Init();

        public abstract bool SetupNextTest(QAgent agent);

        public abstract void OnActionTaken(QAgent agent, SARS sars);

        public abstract void OnTestComplete(double reward);

        public abstract void OnRunComplete();
    }
}
