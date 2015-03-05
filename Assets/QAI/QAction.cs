using System;
using System.Xml.Serialization;
using UnityEngine;
using System.Collections;

public struct QAction {
    public readonly string ActionId;
    [XmlIgnore]
    public readonly Action Action;

    public QAction(string actionId, Action action) : this() {
        ActionId = actionId;
        Action = action;
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
}
