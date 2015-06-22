using System;
using System.Xml.Serialization;

namespace QAI.Agent {
    [Serializable]
    public class QAction {

        public static readonly QAction NullAction = new QAction("0", () => { }, null);

        public readonly string ActionId;
        [XmlIgnore][NonSerialized]
        public readonly Action Action;
        [XmlIgnore][NonSerialized]
        private readonly QPredicate.Basic p;

        public QAction(string actionId, Action action, QPredicate.Basic predicate)  {
            ActionId = actionId;
            Action = action;
            p = predicate;
        }

        public bool IsValid() {
            return p == null || p.Invoke(Action);
        }

        public override bool Equals(object obj) {
            return obj is QAction && ActionId == ((QAction)obj).ActionId;
        }

        public override int GetHashCode() {
            return ActionId.GetHashCode();
        }

        public void Invoke() {
            Action.Invoke();
        }

        public override string ToString ()	{
            return Action.Method.Name;
        }
    }
}
