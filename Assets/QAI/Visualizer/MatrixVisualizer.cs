using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

namespace QAI.Visualizer {
	public class MatrixVisualizer {
		public Texture2D Texture { get; private set; }
        public float MaxValue { get; private set; }

		public MatrixVisualizer(int width, int height) {
			Texture = new Texture2D(width, height) { filterMode = FilterMode.Point};
		}
        
		public void Update(Matrix<float> matrix) {
		    var iterationMax = 0f;
			for(var x = 0; x < matrix.RowCount; x++) {
				for(var y = 0; y < matrix.ColumnCount; y++) {
				    var value = matrix.At(x, y);
                    iterationMax = Mathf.Max(value, iterationMax);
                    value /= Mathf.Max(MaxValue, 1f);
					Texture.SetPixel(x,y, NetworkVisualizer.GetColor(value));
				}
			}
			Texture.Apply();
		    MaxValue = iterationMax;
		}

		
	}
}
