using System;
using System.Xml.Serialization;
using UnityEngine;
using System.Collections;

public class QAction {
    public int ActionIndex;
    [XmlIgnore]
    public Action Action;

    public override bool Equals(object obj) {
        return obj is QAction && ActionIndex == ((QAction)obj).ActionIndex;
    }

    public override int GetHashCode() {
        return ActionIndex.GetHashCode();
    }
}
