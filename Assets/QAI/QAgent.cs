using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public interface QAgent {
    QState GetState();
	Action GetImitationAction();
}

public static class QAgentExtension {
    public static IList<QAction> GetQActions(this QAgent agent) {
        return agent.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(QBehavior), true).Length > 0)
            .Select(m => ((QBehavior)m.GetCustomAttributes(typeof(QBehavior), true).First()).ObtainAction(agent, m.Name)).ToList();
    }

	public static QAction ConvertImitationAction(this QAgent agent) {
		var a = agent.GetImitationAction();
		return agent.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
			.Where (m => m.Name == a.Method.Name)
			.Where(m => m.GetCustomAttributes(typeof(QBehavior), true).Length > 0)
				.Select(m => ((QBehavior)m.GetCustomAttributes(typeof(QBehavior), true).First()).ObtainAction(agent, m.Name)).First();
	}

    public static SARS MakeSARS(this QAgent agent, QAction move) {
        var s = agent.GetState();
        move.Invoke();
        var s0 = agent.GetState();
        return new SARS(s, move, s0.Reward, s0);
    }
}
