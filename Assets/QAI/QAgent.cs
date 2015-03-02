using System;
using System.Collections.Generic;
using System.Linq;

public interface QAgent {
    Action[] GetActions();
    QState GetState();
}
public static class QAgentExtension {
    public static QAction[] GetQActions(this QAgent agent) {
        return agent.GetActions().Select((a, i) => new QAction {ActionIndex = i, Action = a}).ToArray();
    }
}
