using System;
using System.Collections.Generic;
using System.Linq;
using Math = UnityEngine.Mathf;

namespace QNetwork.CNN {
	class ConvolutionalNetwork : Network {
        public ConvolutionalNetwork(int inroot, int outsize, params int[][] filters) {
            layers = new List<Layer>(filters.Length + 2);
            layers.Add(new Layer(inroot * inroot, Utils.Identity));
            var prev = MapTo3D(layers.First(), 1, inroot, inroot); // TODO: Channels.
            for (int i = 0; i < filters.Length; i++) {
                prev = AddConvolutionalLayer(prev, filters[i][0], filters[i][1], 1); // TODO: Stride.
                layers.Add(new Layer(prev.Cast<Neuron>(), Utils.RandomList(prev.Length, -0.1f, 0.1f)));
            }
            var ws = Utils.RandomList(outsize * prev.Length, -1f / Math.Sqrt(prev.Length), 1f / Math.Sqrt(prev.Length));
            layers.Last().AddConnection(new Layer(outsize, Utils.Sigmoid), ws.Cast<Weight>());
            FinalizeStructure();
        }

        private Neuron[, ,] AddConvolutionalLayer(Neuron[, ,] source, int numf, int fsize, int stride) {
            int mapsize = source.GetUpperBound(1) - fsize + 1; // TODO: Stride.
            Neuron[, ,] fmap = PopulateLayer(numf, mapsize, Utils.Tanh);
            for (int c = 0; c < source.GetUpperBound(0); c++) {
                for (int f = 0; f < numf; f++) {
                    Weight[,] filter = CreateFilter(fsize, mapsize * mapsize);
                    for (int m = 0; m < mapsize; m++)
                        for (int n = 0; n < mapsize; n++)
                            for (int u = 0; u < fsize; u++)
                                for (int v = 0; v < fsize; v++)
                                    source[c, m + u, n + v].AddConnection(fmap[f, m, n], filter[u, v].Singleton());
                }
            }
            return fmap;
        }

        private Neuron[, ,] PopulateLayer(int depth, int size, ActivationFunction type) {
            var grid = new Neuron[depth, size, size];
            for (int i = 0; i < depth; i++)
                for (int j = 0; j < size; j++)
                    for (int k = 0; k < size; k++)
                        grid[i, j, k] = new Neuron(type);
            return grid;
        }

        private Weight[,] CreateFilter(int size, int cardinality) {
            var ws = Utils.RandomList(size * size, -1f / Math.Sqrt(cardinality), 1f / Math.Sqrt(cardinality)).ToList();
            var filter = new SharedWeight[size, size];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    filter[i, j] = new SharedWeight(ws[i * size + j], cardinality);
            return filter;
        }

        private T[, ,] MapTo3D<T>(IEnumerable<T> source, int k, int x, int y) {
            var it = source.GetEnumerator();
            T[, ,] arr = new T[k, x, y];
            for (int i = 0; i < k; i++)
                for (int m = 0; m < x; m++)
                    for (int n = 0; n < y; n++) {
                        it.MoveNext();
                        arr[i, m, n] = it.Current;
                    }
            return arr;
        }
	}
}
