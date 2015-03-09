using System;
using System.Collections.Generic;
using System.Linq;
using Debug = UnityEngine.Debug;
using Random = System.Random;

public static class LinqExtension {
    private static readonly Random Rng = new Random();
    private const double Tolerance = 0.000001;
    public static T MaxWithRandomTie<T>(this IEnumerable<T> source, Func<T,double> f) {
        var ordered = source.OrderByDescending(f);
        var high = f(ordered.First());
        var tieies = ordered.Where(x => Math.Abs(f(x) - high) < Tolerance).ToArray();
        if(tieies.Length == 0) 
            Debug.Log(high);
        return tieies[Rng.Next(tieies.Length)];
    }

    
}
