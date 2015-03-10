using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public interface QAgent {
    QState GetState();
}

public static class QAgentExtension {
    public static IList<QAction> GetQActions(this QAgent agent) {
        return agent.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(QBehavior), true).Length > 0)
            .Select(m => ((QBehavior)m.GetCustomAttributes(typeof(QBehavior), true).First()).ObtainAction(agent, m.Name)).ToList();
    }
}
