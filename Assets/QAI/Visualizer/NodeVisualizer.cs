using UnityEngine;
using UnityEngine.UI;

namespace QAI.Visualizer {
    public class NodeVisualizer {
        private readonly GameObject _node;
        private readonly Texture2D _texture;


        public NodeVisualizer() {
            _node = new GameObject("Node");
            var image = _node.AddComponent<RawImage>();
            _texture = new Texture2D(1,1);
            image.texture = _texture;
            image.uvRect = new Rect(0,0,2,2);
        }

        public void Update(float value) {
            _texture.SetPixel(0,0, NetworkVisualizer.GetColor(value));
            _texture.Apply();
        }



        public GameObject GetUI() {
            return _node;
        }
    }
}
