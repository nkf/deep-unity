using UnityEngine;
using System.Collections;
using QNetwork.CNN;
using System.Collections.Generic;
using UnityEngine.UI;

namespace QAI.Visualizer {
	public class NetworkVisualizer : MonoBehaviour {
		private ConvolutionalNetwork _cnn;
		private List<LayerVisualizer> _layers = new List<LayerVisualizer>();

		private void Init(ConvolutionalNetwork cnn) {
			_cnn = cnn;
			DontDestroyOnLoad(gameObject);
			var p = GetComponentInChildren<GridLayoutGroup>();
			foreach(var l in _cnn.IterateSpatialLayers()) {
				var visualLayer = new LayerVisualizer(l);
				_layers.Add(visualLayer);
				var go = visualLayer.CreateUI();
				go.transform.SetParent(p.transform, false);
			}

		}
		
		// Update is called once per frame
		void FixedUpdate () {
			foreach(var l in _layers) l.Update();
		}

		public static NetworkVisualizer CreateVisualizer(ConvolutionalNetwork network) {
			var nv = GameObject.Instantiate(Resources.Load<NetworkVisualizer>("NetworkVisualizer"));
			nv.Init(network);
			return nv;
		}
	}
}
