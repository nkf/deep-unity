using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

public class QAIOptionWindow : EditorWindow {
	private bool _imitation;
	private bool _learning;
	private bool _show = true;
    private int _term;

	// Add menu named "My Window" to the Window menu
	[MenuItem ("QAI/Options")]
	static void Init () {
		// Get existing open window or if none, make a new one:
		QAIOptionWindow window = (QAIOptionWindow)EditorWindow.GetWindow (typeof (QAIOptionWindow));
		var ais = GameObject.FindObjectsOfType<QAI>();
		window._imitation = ais.All(q => q.IMITATING);
		window._learning = ais.All (q => q.LEARNING);
		window.Show();
	}

	void OnGUI() {
		EditorGUILayout.HelpBox("To train the AI, turn on learning. You can leave this on during play to have the AI adapt over time to the way the user is playing", MessageType.None);
		_learning = EditorGUILayout.ToggleLeft("Learning", _learning);
		if(_learning) {
            _term = EditorGUILayout.IntField("Terminate after # episodes", _term);
			if(_show = EditorGUILayout.Foldout(_show, "Imitation Learning")) {
				EditorGUI.indentLevel++;
				EditorGUILayout.HelpBox("It is possible for the developer to teach the AI the first steps of how to play the game. Implement the method GetImitationAction to send input to the AI and QAI.Imitate to tell the AI that new input is available.", MessageType.Info);
				_imitation = EditorGUILayout.Toggle("Learn from player input", _imitation);
				EditorGUI.indentLevel--;
			}
		}
		foreach(var ai in GameObject.FindObjectsOfType<QAI>()) {
			ai.IMITATING = _imitation;
			ai.LEARNING = _learning;
            ai.TERMINATOR = _term;
		}
	}
}

