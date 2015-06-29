using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using QNetwork.CNN;
using QNetwork.MLP;
using UnityEngine;
using UnityEngine.UI;

namespace QAI.Visualizer {
    public class DenseLayerVisualizer {
        private readonly SpatialLayer _spatialLayer;
        private DenseLayer _denseLayer;
        private List<NodeMatrixVisualizer> _nodeMatrices;
        private List<OutputNodeVisualizer> _outputNodes;
        private GameObject _spatialVisuals;
        private GameObject _outputVisuals;
        private string[] _actionIndex;

        public DenseLayerVisualizer(SpatialLayer spatialLayer, DenseLayer denseLayer, string[] actionIndex) {
            _actionIndex = actionIndex;
            _spatialLayer = spatialLayer;
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
        }
    }
}
