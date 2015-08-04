using MathNet.Numerics.LinearAlgebra;
using UnityEngine;
using UnityEngine.UI;

namespace QAI.Visualizer {
    public class NodeVectorVisualizer {
        private readonly GameObject _nodeVector;
        private readonly NodeVisualizer[] _nodes;
        public int Size { get { return _nodes.GetLength(0); } }

        public NodeVectorVisualizer(int size) {
			_nodeVector = GameObject.Instantiate(Resources.Load<GameObject>("NodeMatrix"));
			_nodeVector.GetComponent<GridLayoutGroup>().constraintCount = size;
            _nodes = new NodeVisualizer[size];
            for (int x = 0; x < size; x++) {
                var nv = new NodeVisualizer();
				nv.GetUI().transform.SetParent(_nodeVector.transform, false);
                _nodes[x] = nv;
            }
        }

        public Vector3 GetNodePosition(int x) {
            return _nodes[x].GetUI().transform.position;
        }

        public GameObject GetUI() {
			return _nodeVector;
        }

        public void Update(Vector<float> vector) {
            for (int x = 0; x < _nodes.GetLength(0); x++) {
                _nodes[x].Update( vector.At(x) );
            }
        }
    }
}
