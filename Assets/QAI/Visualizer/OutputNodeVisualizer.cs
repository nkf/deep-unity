using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace QAI.Visualizer {
    public class OutputNodeVisualizer {
        private readonly GameObject _outputNode;
        private readonly GameObject _selected;
        private readonly Texture2D _texture;
        public Vector3 Position { get { return _outputNode.transform.position; } }


        public OutputNodeVisualizer(string actionName) {
            _outputNode = GameObject.Instantiate(Resources.Load<GameObject>("OutputNodeVisualizer"));
            var images = _outputNode.GetComponentsInChildren<RawImage>();
            _selected = images.First(i => i.gameObject.name == "Selected").gameObject;
            _texture = new Texture2D(1, 1);
            var value = images.First(i => i.gameObject.name == "Value");
            value.texture = _texture;
            value.uvRect = new Rect(0,0,2,2);
            _outputNode.GetComponentInChildren<Text>().text = actionName;
        }
        

        public GameObject GetUI() {
            return _outputNode;
        }

        public void Update(float value, bool selected) {
            _selected.SetActive(selected);
            _texture.SetPixel(0,0, NetworkVisualizer.GetColor(value));
            _texture.Apply();
        }

    }
}
