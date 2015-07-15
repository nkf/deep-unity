using System;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

public static class LinqExtension {
    private static readonly Random Rng = new Random();

    public static int IndexOfMax(this IEnumerable<float> source) {
        return !source.Any() ? -1 : source.Select((v, i) => new { Value = v, Index = i })
            .Aggregate((a, b) => (a.Value > b.Value) ? a : b).Index;
    }

    public static IEnumerable<double> Normalize(this IEnumerable<double> source, double min, double max) {
        return source.Select(x => (x - min) / (max - min));
    }

    public static IEnumerable<T> Random<T>(this IEnumerable<T> source) {
        if (!source.Any()) yield break;
        var n = Rng.Next(source.Count());
        yield return source.ElementAt(n);
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source) {
        var buffer = source.ToList();
        for (int i = 0; i < buffer.Count; i++) {
            int j = Rng.Next(i, buffer.Count);
            yield return buffer[j];
            buffer[j] = buffer[i];
        }
    }

    public static void Swap<T>(this IList<T> l, int a, int b) {
        if (a < 0 || a >= l.Count || b < 0 || b > l.Count) return;
        var temp = l[a];
        l[a] = l[b];
        l[b] = temp;
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
        foreach (T t in source)
            action(t);
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action) {
        int i = 0;
        foreach (T t in source)
            action(t, i++);
    }

    public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int parts) {
        int i = 0;
        return from item in source
               group item by i++ % parts into part
               select part.AsEnumerable();
    }

    public static IEnumerable<List<T>> Partition<T>(IList<T> source, int parts) {
        var max = source.Count/parts;
        max = Math.Max(max, 1);
        var evenLists = source.Count%parts; //the number of lists which will have an extra element in
        var toReturn = new List<T>(max);
        var n = 0;
        foreach(var item in source) {
            toReturn.Add(item);
            if(toReturn.Count == max) {
                yield return toReturn;
                n++;
                if (n == parts - evenLists && evenLists != 0) max++;
                toReturn = new List<T>(max);
            }
        }
    }
}
