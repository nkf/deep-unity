using UnityEngine;
using System.Collections;
using QNetwork;
using MathNet.Numerics.LinearAlgebra;

namespace QAI.Visualizer {
	public class MatrixVisualizer {
		public Texture2D Texture { get; private set; }

		public MatrixVisualizer(int width, int height) {
			Texture = new Texture2D(width, height) { filterMode = FilterMode.Point};
		}

		public void Update(Matrix<float> matrix) {
			for(var x = 0; x < matrix.RowCount; x++) {
				for(var y = 0; y < matrix.ColumnCount; y++) {
					Texture.SetPixel(x,y, GetColor(matrix[x,y]));
				}
			}
			Texture.Apply();
		}

		private Color GetColor(float value) {
			return value > 0 ? Color.Lerp(Color.black, Color.white, value) : Color.Lerp(Color.black, Color.red, Mathf.Abs(value));
		}
	}
}
