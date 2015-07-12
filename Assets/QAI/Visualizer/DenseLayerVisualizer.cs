using System;
using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using QNetwork.CNN;
using QNetwork.Experimental;
using QNetwork.MLP;
using UnityEngine;
using UnityEngine.UI;

namespace QAI.Visualizer {
    public class DenseLayerVisualizer {
        private readonly SpatialLayer _spatialLayer;
        private readonly TreeLayer _combinationLayer;
        private readonly DenseLayer _denseLayer;
        private List<NodeMatrixVisualizer> _nodeMatrices;
        private List<OutputNodeVisualizer> _outputNodes;
        private readonly GameObject _spatialVisuals;
        private readonly GameObject _outputVisuals;
        private readonly string[] _actionIndex;

        public DenseLayerVisualizer(SpatialLayer spatialLayer,TreeLayer combinationLayer, DenseLayer denseLayer, string[] actionIndex) {
            _actionIndex = actionIndex;
            _spatialLayer = spatialLayer;
            _combinationLayer = combinationLayer;
            _denseLayer = denseLayer;
            _spatialVisuals = GameObject.Instantiate(Resources.Load<GameObject>("DenseLayerVisualizer"));
            _outputVisuals = GameObject.Instantiate(Resources.Load<GameObject>("OutputLayerVisualizer"));
        }

        private void InitInput(Matrix<float>[] input) {
            _nodeMatrices = new List<NodeMatrixVisualizer>();
            var p = _spatialVisuals.GetComponentInChildren<GridLayoutGroup>();
            foreach (var matrix in input) {
                var nmv = new NodeMatrixVisualizer(matrix.RowCount);
                nmv.GetUI().transform.SetParent(p.transform, false);
                _nodeMatrices.Add(nmv);
            }
        }

        private void InitOutput(Vector<float> output) {
            _outputNodes = new List<OutputNodeVisualizer>();
            var p = _outputVisuals.GetComponentInChildren<GridLayoutGroup>();
            for (int i = 0; i < output.Count; i++) {
                var onv = new OutputNodeVisualizer(_actionIndex[i]);
                onv.GetUI().transform.SetParent(p.transform, false);
                _outputNodes.Add(onv);
            }
        }

        public GameObject[] GetUI() {
			return new []{ _spatialVisuals, _outputVisuals };
        }

        private void setBackgroundColor(Color color) {
            _spatialVisuals.GetComponent<Image>().color = color;
            _outputVisuals.GetComponent<Image>().color = color;
        }

        public void Update(bool isTrainingData) {
            setBackgroundColor(isTrainingData ? NetworkVisualizer.TrainingColor : NetworkVisualizer.IdleColor);
            var input = _spatialLayer.Output();
            if(input == null || input.Length == 0) return;
            if(_nodeMatrices == null) InitInput(input);

            for(var i = 0; i < input.Length; i++) {
                _nodeMatrices[i].Update(input[i]);
            }
            
            var output = _denseLayer.Output();
            if(output == null || output.Count == 0) return;
            if(_outputNodes == null) InitOutput(output);

            var max = output.MaximumIndex();
            for(var i = 0; i < output.Count; i++) {
                _outputNodes[i].Update(output[i], i == max);
            }

            var weights = _denseLayer.Weights;
            var connections = SortedIndices(weights);

            var matrixSize = _nodeMatrices[0].Size;
            var matrixCount = matrixSize*matrixSize;
            var maxN = matrixCount*_nodeMatrices.Count;
            var maxValue = Mathf.Abs(connections[0].Weight);
            int n = 0;
            var from = Vector3.zero;
            while (maxValue - Mathf.Abs(connections[n].Weight) < 1f && n < 25) {
                var connection = connections[n++];
                if (connection.FromIndex < _combinationLayer.LeftSize) {
                    var nodeIndex = CalculateNodeIndex(connection.FromIndex, matrixSize, matrixCount);
                    from = _nodeMatrices[nodeIndex.Item1].GetNodePosition(nodeIndex.Item2, nodeIndex.Item3);
                }
                var to = _outputNodes[connection.ToIndex].Position;
                Draw2D.DrawLine(from, to, ConnectionColor(connection.Weight), 2, false);
            }
        }

        private List<Connection> SortedIndices(Matrix<float> matrix) {
            var list = new List<Connection>();
            for (int x = 0; x < matrix.RowCount; x++) {
                for (int y = 0; y < matrix.ColumnCount; y++) {
                    list.Add(new Connection(y,x, matrix.At(x,y)));
                }
            }
            list.Sort((c1,c2) => Mathf.Abs(c2.Weight).CompareTo(Mathf.Abs(c1.Weight)));
            return list;
        }

        private Tuple<int, int, int> CalculateNodeIndex(int index, int matrixSize, int matrixCount) {
            var matrixIndex = index/matrixCount;
            var nIndex = index - matrixCount*matrixIndex;
            var x = nIndex%matrixSize;
            var y = nIndex/matrixSize;
            return new Tuple<int, int, int>(matrixIndex,x,y);
        }

        private Color ConnectionColor(float value) {
            return value > 0 ? Color.Lerp(Color.white, Color.green, value) : Color.Lerp(Color.white, Color.red, Mathf.Abs(value));
        }

        private struct Connection {
            public readonly int FromIndex;
            public readonly int ToIndex;
            public readonly float Weight;
            public Connection(int from, int to, float weight) {
                FromIndex = from;
                ToIndex = to;
                Weight = weight;
            }
        }
    }
}
