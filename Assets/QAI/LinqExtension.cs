using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public static class LinqExtension {
    public static T MaxWithRandomTie<T>(this IEnumerable<T> source, Func<T,double> f) {
        var ordered = source.OrderByDescending(f);
        var high = f(ordered.First());
        var tieies = ordered.Where(x => f(x) == high).ToArray();
        return tieies[Random.Range(0, tieies.Length)];
    }
}
