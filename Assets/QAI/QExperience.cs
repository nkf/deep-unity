﻿using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Xml.Serialization;

public class QExperience : IEnumerable<SARS> {
    private List<SARS> _data = new List<SARS>();

    public void Store(SARS sars, int maxSize) {
        _data.Add(sars);
        if (_data.Count > maxSize)
            _data.RemoveAt(0); // TODO: Expensive on List.
    }

    public void Store(SARS sars) {
        _data.Add(sars);
    }

    public IEnumerator<SARS> GetEnumerator() {
        return _data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}
