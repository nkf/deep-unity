using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using QNetwork.CNN;
using UnityEngine.UI;
using MathNet.Numerics.LinearAlgebra;

namespace QAI.Visualizer {
	public class LayerVisualizer  {
		private const float V_SCALE = 2f;

		private List<MatrixVisualizer> _images;
		private SpatialLayer _layer;
		private GameObject _visuals;

		public LayerVisualizer(SpatialLayer layer) {
			_layer = layer;
		}

		private void Init(Matrix<float>[] o) {
			_images = new List<MatrixVisualizer>();
			var p = _visuals.GetComponentInChildren<GridLayoutGroup>();
			Matrix<float> last = null;
			foreach(var m in o) {
				var mv = new MatrixVisualizer(m.RowCount, m.ColumnCount);
				var r = GameObject.Instantiate(Resources.Load<RawImage>("MatrixImage"));
				r.texture = mv.Texture;
				r.transform.SetParent(p.transform, false);

				_images.Add(mv);
				last = m;
			}

			if(last != null)
				p.cellSize = new Vector2(last.RowCount * V_SCALE, last.ColumnCount * V_SCALE);
		}

		public GameObject CreateUI() {
			_visuals = GameObject.Instantiate(Resources.Load<GameObject>("LayerVisualizer"));
			_visuals.GetComponentInChildren<Text>().text = _layer.GetType().Name;
			return _visuals;
		}

		public void Update() {
			var o = _layer.Output ();
			if(o == null || o.Length == 0) return;
			if(_images == null) Init(o);

			for(var i = 0; i < o.Length; i++) {
				_images[i].Update(o[i]);
			}
		}
	}
}
