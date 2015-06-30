﻿using UnityEngine;
using System.Collections.Generic;
using QNetwork.CNN;
using UnityEngine.UI;
using MathNet.Numerics.LinearAlgebra;

namespace QAI.Visualizer {
	public class ConvLayerVisualizer  {

		private List<MatrixVisualizer> _images;
		private SpatialLayer _layer;
		private GameObject _visuals;

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
            _visuals.GetComponentInChildren<Text>().text = _layer.GetType().Name.Replace("Layer", "") + info;
		}

		public GameObject CreateUI() {
			_visuals = GameObject.Instantiate(Resources.Load<GameObject>("ConvLayerVisualizer"));
			return _visuals;
		}

		public void Update() {
			var o = _layer.Output();
			if(o == null || o.Length == 0) return;
			if(_images == null) Init(o);

			for(var i = 0; i < o.Length; i++) {
				_images[i].Update(o[i]);
			}
		}
	}
}