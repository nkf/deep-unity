using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public interface QAgent {
    QState GetState();
}

public static class QAgentExtension {
    public static QAction[] GetQActions(this QAgent agent) {
        int i = 0;
        return
            (from m in agent.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            where m.GetCustomAttributes(typeof(QBehavior), true).Length > 0
            select new QAction {
                ActionIndex = i++,
                Action = (Action)Delegate.CreateDelegate(typeof(Action), agent, m.Name)
            }).ToArray();
    }
}
