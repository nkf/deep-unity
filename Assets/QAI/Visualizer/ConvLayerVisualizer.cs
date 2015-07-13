using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using QNetwork.CNN;
using UnityEngine.UI;
using MathNet.Numerics.LinearAlgebra;

namespace QAI.Visualizer {
	public class ConvLayerVisualizer  {

		private List<MatrixVisualizer> _images;
		private SpatialLayer _layer;
		private GameObject _visuals;
	    private Text _text;
	    private string _info;

		public ConvLayerVisualizer(SpatialLayer layer) {
			_layer = layer;
		}

		private void Init(Matrix<float>[] o) {
			_images = new List<MatrixVisualizer>();
			var p = _visuals.GetComponentInChildren<GridLayoutGroup>();

			foreach(var m in o) {
				var mv = new MatrixVisualizer(m.RowCount, m.ColumnCount);
				var r = GameObject.Instantiate(Resources.Load<RawImage>("MatrixImage"));
				r.texture = mv.Texture;
				r.transform.SetParent(p.transform, false);

				_images.Add(mv);
			}
		    var info = string.Format("({0}x{1}x{2})", o[0].RowCount, o[0].ColumnCount, o.Length);
            if (_layer is ConvolutionalLayer) {
		        var convLayer = _layer as ConvolutionalLayer;
		        info += string.Format(" fsize: {0} stride: {1}", convLayer.FilterSize, convLayer.Stride);
		    }
		    if (_layer is MeanPoolLayer) {
		        var meanLayer = _layer as MeanPoolLayer;
		        info += string.Format(" psize: {0}", meanLayer.PoolSize);
		    }
		    if (_layer is MaxPoolLayer) {
		        var maxLayer = _layer as MaxPoolLayer;
                info += string.Format(" psize: {0}", maxLayer.PoolSize);
		    }
            _info = _layer.GetType().Name.Replace("Layer", "") + info;
		    _text = _visuals.GetComponentInChildren<Text>();
		    _text.text = _info;


		}

		public GameObject CreateUI() {
			_visuals = GameObject.Instantiate(Resources.Load<GameObject>("ConvLayerVisualizer"));
			return _visuals;
		}

	    private void setBackgroundColor(Color color) {
	        _visuals.GetComponent<Image>().color = color;
	    }

		public void Update(bool isTrainingData) {
            setBackgroundColor(isTrainingData ? NetworkVisualizer.TrainingColor : NetworkVisualizer.IdleColor);
			var o = _layer.Output();
			if(o == null || o.Length == 0) return;
			if(_images == null) Init(o);


			for(var i = 0; i < o.Length; i++) {
				_images[i].Update(o[i]);
			}
		    var fmax = _images.Select(i => string.Format("{0:F}", i.MaxValue)).ToArray();
		    _text.text = _info + " max (" + string.Join(",", fmax) + ")";
		}
	}
}
