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

        public void Update() {
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
            var n = weights.RowCount * weights.ColumnCount * 0.10f;

            var matrixSize = _nodeMatrices[0].Size;
            var matrixCount = matrixSize*matrixSize;
            for (int i = 0; i < n; i++) {
                var connection = connections[i];
                var nodeIndex = CalculateNodeIndex(connection.Item2, matrixSize, matrixCount);
                var from = _nodeMatrices[nodeIndex.Item1].GetNodePosition(nodeIndex.Item2, nodeIndex.Item3);
                var to = _outputNodes[connection.Item1].Position;
                Draw2D.DrawLine(from,to,ConnectionColor(connection.Item3),2,false);
            }
        }

        private List<Tuple<int,int,float>> SortedIndices(Matrix<float> matrix) {
            var list = new List<Tuple<int, int, float>>();
            for (int x = 0; x < matrix.RowCount; x++) {
                for (int y = 0; y < matrix.ColumnCount; y++) {
                    if(y >= _combinationLayer.LeftSize) continue;
                    list.Add(new Tuple<int, int, float>(x,y, matrix.At(x,y)));
                }
            }
            list.Sort((t1,t2) => Mathf.Abs(t2.Item3).CompareTo(Mathf.Abs(t1.Item3)));
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
    }
}
