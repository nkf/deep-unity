using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[Serializable]
public class QExperience : IEnumerable<SARS> {
    private readonly List<SARS> _data = new List<SARS>();

    public void Store(SARS sars, int maxSize) {
        Store(sars);
        if (_data.Count > maxSize)
            _data.RemoveAt(0); // TODO: Expensive on List.
    }

    public void Store(SARS sars) {
        if (sars.State.Equals(sars.NextState)) return;
        _data.Add(sars);
    }

    public void Add(SARS sars) {
        Store(sars);
    }

    public int Count { get { return _data.Count; }}

    public SARS this[int i] {
        get { return _data[i]; } 
        set { _data[i] = value; }
    }

    public IEnumerator<SARS> GetEnumerator() {
        return _data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}
