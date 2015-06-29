using UnityEngine;
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
					Texture.SetPixel(x,y, NetworkVisualizer.GetColor(matrix[x,y]));
				}
			}
			Texture.Apply();
		}

		
	}
}
