using System.Collections.Generic;
using Encog.ML.Data;

public static class IMLDataExtension {
    public static IEnumerable<double> ToEnumerable(this IMLData data) {
        for (int i = 0; i < data.Count; i++)
            yield return data[i];
    }
}
