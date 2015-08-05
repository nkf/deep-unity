using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using QNetwork.CNN;
using QAI;
using MathNet.Numerics.LinearAlgebra;

public class QAIGraphWindow : EditorWindow {
	private bool foldoutActions = false;

	private LineChart actionChart;

	private List<Vector<float>> actionData = new List<Vector<float>>();
	private int maxActionData = 100;

	[MenuItem("QAI/Data")]
	private static void OpenWindow() {
		var window = (QAIGraphWindow) GetWindow(typeof (QAIGraphWindow));
		window.Show();
	}

	private void Init() {
		QAIManager.NetworkValuesUpdated += StoreValues;
	}

	private void OnGUI() {
		Init ();

		if(foldoutActions = EditorGUILayout.Foldout(foldoutActions, "Action taken")) {
			if(actionChart == null) {
				actionChart = new LineChart(this, 200f);
				actionChart.formatString = "{0:F}";
				actionChart.gridLines = 5;
				actionChart.axisRounding = 1f;
			}

			List<List<float>> l = null;
			foreach(var v in actionData) {
				if(l == null) {
					l = new List<List<float>>();
					foreach(var e in v) l.Add(new List<float>());
				}
				for(var i = 0; i < v.Count; i++) {
					l[i].Add(v[i]);
				}
			}
			if(l == null) return;

			actionChart.data = l.ToArray();
//			actionChart.dataLabels = new List<string>{"Hest", "Ko"};
			actionChart.DrawChart();
		}
	}

	private void StoreValues(Vector<float> data, bool trainingData) {
		if(trainingData) return;
		if(actionData.Count > maxActionData) actionData.Remove(actionData.First());
		if(actionData.Count != 0 && actionData[actionData.Count-1].Equals(data)) return;
		actionData.Add(data.Clone());
		Repaint();
	}
}
