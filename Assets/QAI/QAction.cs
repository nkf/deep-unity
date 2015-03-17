using System;
using System.Xml.Serialization;
using UnityEngine;
using System.Collections;

public struct QAction {
    public readonly string ActionId;
    [XmlIgnore]
    public readonly Action Action;
    [XmlIgnore]
    private readonly QPredicate.Basic p;

    public QAction(string actionId, Action action, QPredicate.Basic predicate) : this() {
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
