using System;
using System.Linq;
using System.Reflection;

[AttributeUsage(AttributeTargets.Method)]
public class QBehavior : Attribute {

    private readonly bool _conditional = false;
    private readonly string _predicate;

    public QBehavior() {
    }

    public QBehavior(string predicate) {
        _conditional = true;
        _predicate = predicate;
    }

    public QAction ObtainAction(QAgent agent, string name) {
        if (_conditional) {
            var p =
                agent.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(m => m.Name == _predicate);
            if (p == default(MethodInfo))
                throw new Exception("Predicate method " + _predicate + " does not exist.");
            if (p.GetCustomAttributes(typeof (QPredicate), true).Length == 0)
                throw new Exception("Predicate method " + _predicate + " is not properly annotated as a QPredicate.");
        }
        return new QAction(
            name,
            (Action)Delegate.CreateDelegate(typeof(Action), agent, name),
            _conditional ? (QPredicate.Basic)Delegate.CreateDelegate(typeof(QPredicate.Basic), agent, _predicate) : null
        );
    }
}
