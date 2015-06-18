using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public interface QAgent {
    QState GetState();
    AIID AI_ID();
}

public static class QAgentExtension {
    public static List<QAction> GetQActions(this QAgent agent) {
        return agent.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(QBehavior), true).Length > 0)
            .Select(m => ((QBehavior)m.GetCustomAttributes(typeof(QBehavior), true).First()).ObtainAction(agent, m.Name)).ToList();
    }

    public static QAction ToQAction(this QAgent agent, Action a) {
        return agent.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
			.Where (m => m.Name == a.Method.Name)
			.Where(m => m.GetCustomAttributes(typeof(QBehavior), true).Length > 0)
			.Select(m => ((QBehavior)m.GetCustomAttributes(typeof(QBehavior), true).First()).ObtainAction(agent, m.Name)).First();
    }

    public static SARS MakeSARS(this QAgent agent, QAction move) {
        var s = agent.GetState();
        return MakeSARS(agent, move, s);
    }

    public static SARS MakeSARS(this QAgent agent, QAction move, QState state) {
        move.Invoke();
        var s0 = agent.GetState();
        return new SARS(state, move, s0);
    }
}
