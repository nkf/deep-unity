using System;

namespace QAI.Agent {
    [AttributeUsage(AttributeTargets.Method)]
    public class QPredicate : Attribute {
        public delegate bool Basic(Action a);
        public delegate bool Conditional(Func<bool> f);
    }
}
