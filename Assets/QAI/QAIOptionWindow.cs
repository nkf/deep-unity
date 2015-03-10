using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

public class QAIOptionWindow : EditorWindow {
	private bool _imitation;
	private bool _show = true;

	// Add menu named "My Window" to the Window menu
	[MenuItem ("QAI/Options")]
	static void Init () {
		// Get existing open window or if none, make a new one:
		QAIOptionWindow window = (QAIOptionWindow)EditorWindow.GetWindow (typeof (QAIOptionWindow));
		window._imitation = GameObject.FindObjectsOfType<QAI>().All(q => q.ImitationProcess);
		window.Show();
	}

	void OnGUI() {
		if(_show = EditorGUILayout.Foldout(_show, "Imitation Learning")) {	
			_imitation = EditorGUILayout.Toggle("Learn on play", _imitation);
		}
		foreach(var ai in GameObject.FindObjectsOfType<QAI>()) {
			ai.ImitationProcess = _imitation;
		}
	}
}

