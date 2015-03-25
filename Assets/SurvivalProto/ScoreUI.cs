using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour {
    private Text _text;
    private GridMan _man;
    void Awake() {
        _text = GetComponent<Text>();
        _man = FindObjectOfType<GridMan>();
    }
    void Update() {
        _text.text = "Score: "+_man.Score +"\n"+
                     "HP: "+_man.HP;
    }
}
