using System.Linq;
using UnityEngine;
using QNetwork.CNN;
using System.Collections.Generic;
using UnityEngine.UI;

namespace QAI.Visualizer {
	public class NetworkVisualizer : MonoBehaviour {
        public static Color TrainingColor = new Color(0.196f, 0.541f, 0.557f, 0.4f);
        public static Color IdleColor = new Color(1,1,1,0.4f);

		private ConvolutionalNetwork _cnn;
		private List<ConvLayerVisualizer> _convLayers = new List<ConvLayerVisualizer>();
	    private DenseLayerVisualizer _denseLayer;

		private void Init(ConvolutionalNetwork cnn, string[] actionIndex) {
			_cnn = cnn;
			DontDestroyOnLoad(gameObject);
			var p = GetComponentInChildren<GridLayoutGroup>();
		    _convLayers = _cnn.IterateSpatialLayers().Select(sl => new ConvLayerVisualizer(sl)).ToList();
			foreach(var l in _convLayers)
				l.CreateUI().transform.SetParent(p.transform, false);
            _denseLayer = new DenseLayerVisualizer(_cnn.IterateSpatialLayers().Last(), _cnn.CombinationLayer, _cnn.OutputLayer, actionIndex);
		    foreach (var ui in _denseLayer.GetUI()) {
		        ui.transform.SetParent(p.transform,false);
		    }
		}

		void OnGUI () {
		    foreach (var l in _convLayers) l.Update(_cnn.IsOutputFromTraining);
		    _denseLayer.Update(_cnn.IsOutputFromTraining);
		}

		public static NetworkVisualizer CreateVisualizer(ConvolutionalNetwork network, string[] actionIndex) {
			var nv = GameObject.Instantiate(Resources.Load<NetworkVisualizer>("NetworkVisualizer"));
			nv.Init(network, actionIndex);
			return nv;
		}

        public static Color GetColor(float value) {
            return value > 0 ? Color.Lerp(Color.black, Color.white, value) : Color.Lerp(Color.black, Color.red, Mathf.Abs(value));
        }
	}
}
