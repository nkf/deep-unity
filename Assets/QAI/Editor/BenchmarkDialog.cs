using UnityEngine;
using System.Collections;
using UnityEditor;
using QAI;

public class BenchmarkDialog : EditorWindow {
	QAIOptionWindow _optionWindow;
	QAIManager _manager;

	public static void OpenWindow(QAIOptionWindow owner, QAIManager manager) {
		var w = (BenchmarkDialog) GetWindow(typeof (BenchmarkDialog), true, "Benchmark");
		var p = w.position;
		w._optionWindow = owner;
		w._manager = manager;
		p.x = Screen.currentResolution.width/2f - p.width/2f;
		p.y = Screen.currentResolution.height/2f - p.height;
		p.height = 100;
		w.position = p;
		w.ShowPopup();
	}

	private void OnGUI() {
		EditorGUILayout.LabelField("You are about to start benchmarking. Be advised that this can take several hours to complete.", EditorStyles.wordWrappedLabel);
		_manager.BenchmarkID = EditorGUILayout.TextField("Benchmark report name", _manager.BenchmarkID);
		_manager.BenchmarkRuns = EditorGUILayout.IntField("Benchmark rounds", _manager.BenchmarkRuns);
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Close")) {
			_optionWindow.Reset();
			Close();
		}
		if(GUILayout.Button("Start")) {
			if(_manager.BenchmarkID.Equals("")) {
				EditorUtility.DisplayDialog("Ups", "You cannot save a benchmark report without a name", "OK");
			} else {
				_optionWindow.ChangePlayMode();
				Close ();
			}
		}
		GUILayout.EndHorizontal();
	}
}
