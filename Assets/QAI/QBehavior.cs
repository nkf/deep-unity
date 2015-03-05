using System;
using System.Linq;
using System.Reflection;

[AttributeUsage(AttributeTargets.Method)]
public class QBehavior : Attribute {

    private bool conditional = false;
    private string predicate;

    public QBehavior() {
    }

    public QBehavior(string predicate) {
        conditional = true;
        this.predicate = predicate;
    }

    public QAction ObtainAction(QAgent agent, string name) {
        var p = agent.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(m => m.Name == predicate).FirstOrDefault();
        if (p == null)
            throw new Exception("Predicate method " + predicate + " does not exist.");
        if (p.GetCustomAttributes(typeof(QPredicate), true).Length == 0)
            throw new Exception("Predicate method " + predicate + " is not properly annotated as a QPredicate.");
        return new QAction(
            name,
            (Action)Delegate.CreateDelegate(typeof(Action), agent, name),
            conditional ? (QPredicate.Basic)Delegate.CreateDelegate(typeof(QPredicate.Basic), agent, predicate) : null
        );
    }
}
