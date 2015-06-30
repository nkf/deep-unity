using MathNet.Numerics.LinearAlgebra;
using UnityEngine;
using UnityEngine.UI;

namespace QAI.Visualizer {
    public class NodeMatrixVisualizer {
        private GameObject _nodeMatrix;
        private NodeVisualizer[,] _nodes; 

        public NodeMatrixVisualizer(int size) {
            _nodeMatrix = GameObject.Instantiate(Resources.Load<GameObject>("NodeMatrix"));
            _nodeMatrix.GetComponent<GridLayoutGroup>().constraintCount = size;
            _nodes = new NodeVisualizer[size,size];
            for (int x = 0; x < size; x++) {
                for (int y = 0; y < size; y++) {
                    var nv = new NodeVisualizer();
                    nv.GetUI().transform.SetParent(_nodeMatrix.transform, false);
                    _nodes[x, y] = nv;
                }
            }
        }

        public GameObject GetUI() {
            return _nodeMatrix;
        }

        public void Update(Matrix<float> matrix) {
            for (int x = 0; x < _nodes.GetLength(0); x++) {
                for (int y = 0; y < _nodes.GetLength(1); y++) {
                    _nodes[x,y].Update( matrix.At(x,y) );
                }
            }
        }
    }
}
