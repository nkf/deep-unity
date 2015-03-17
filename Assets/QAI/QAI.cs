using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections;

public class QAI : MonoBehaviour {
    public delegate void EpisodeCallback();

    public const float TimeStep = 1f;
    [HideInInspector]
    public bool Learning;
    [HideInInspector]
    public bool Remake;
	[HideInInspector]
	public bool Imitating;
    [HideInInspector]
    public int Terminator;
	[HideInInspector]
	public bool ExperienceReplay;
    [HideInInspector]
    public List<QStory> Stories; 
	
	public GameObject ActiveAgent;
	public int Iteration { get { return _qlearning == null ? 0 : _qlearning.Iteration; }}

    private static QAI _instance = null;
    private QLearningNN _qlearning;

    private QImitation _imitation;

    public void EndOfEpisode() {
        if (_qlearning.Iteration > Terminator) {
            _qlearning.SaveModel();
            Application.Quit();
        } else {
            Application.LoadLevel(Application.loadedLevel);
        }
    }

    private IEnumerator<YieldInstruction> RunAgent() {
        while (true) {
            _qlearning.BestAction().Invoke();
            yield return new WaitForSeconds(TimeStep);
        }
    }

	public static void Imitate(QAgent agent) {
        var terminal = _instance._imitation.Imitate(agent);
	    if (terminal) {
	        _instance._imitation.Save();
	        EditorApplication.isPlaying = false;
	    }
	}

    void Awake() {
        if (_instance == null) {
            _instance = this;
            var woman = ActiveAgent.GetComponent<QAgent>();
			if(Imitating) {
				_imitation = new QImitation();
			} else {
				_qlearning = new QLearningNN(woman);
				if (Learning) {
					if (Remake)
						_qlearning.RemakeModel();
					else
						_qlearning.LoadModel();
					DontDestroyOnLoad(gameObject);
					StartCoroutine(_qlearning.RunEpisode(woman, EndOfEpisode));
				} else {
					_qlearning.LoadModel();
					StartCoroutine(RunAgent());
				}
			}
        } else {
            _instance.ActiveAgent = this.ActiveAgent;
            var woman = ActiveAgent.GetComponent<QAgent>(); // TODO: Multiple agents.
			if(!Imitating)
            	_instance.StartCoroutine(_instance._qlearning.RunEpisode(woman, _instance.EndOfEpisode));
            Destroy(gameObject);
        }
    }
}
