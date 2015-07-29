using UnityEngine;
using System.Collections;
using UnityEditor;

public class QAIGraphWindow : EditorWindow {
	private bool foldoutActions = false;

	private LineChart actionChart;

	[MenuItem("QAI/Data")]
	private static void OpenWindow() {
		var window = (QAIGraphWindow) GetWindow(typeof (QAIGraphWindow));
		window.Show();
	}

	private void OnGUI() {
		if(foldoutActions = EditorGUILayout.Foldout(foldoutActions, "Action taken")) {

		}
	}
}
