using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace QAI.Visualizer {
    public class OutputNodeVisualizer {
        private readonly GameObject _outputNode;
        private readonly GameObject _selected;
        private readonly Text _value;
        public Vector3 Position { get { return _outputNode.transform.position; } }


        public OutputNodeVisualizer(string actionName) {
            _outputNode = GameObject.Instantiate(Resources.Load<GameObject>("OutputNodeVisualizer"));
            var images = _outputNode.GetComponentsInChildren<RawImage>();
            _selected = images.First(i => i.gameObject.name == "Selected").gameObject;
            var texts = _outputNode.GetComponentsInChildren<Text>();
            texts.First(t => t.gameObject.name == "ActionName").text = actionName;
            _value = texts.First(t => t.gameObject.name == "Value");
        }
        

        public GameObject GetUI() {
            return _outputNode;
        }

        public void Update(float value, bool selected) {
            _selected.SetActive(selected);
            _value.text = string.Format("{0:F}",value);
        }

    }
}
