using System.Xml;
using UnityEngine;
using System.Collections;

public class Lava : MonoBehaviour {
    public const double MaxLevel = 5;
    public const double HalfLevel = MaxLevel/2;
    public const double QuaterLevel = MaxLevel/4;
    public const double ThreeQuaterLevel = QuaterLevel*3;
    private Material _material;

    private Material Material {
        get { return _material ?? (_material = GetComponent<Renderer>().material); }
    }

    public bool IsLethal { get { return _level > QuaterLevel; } }

    private Color _color;
    private Color _emission;
    void Awake() {
        _color = Material.color;
        _emission = Material.GetColor("_EmissionColor");
    }
    
    private double _level;
    public double Level {
        get { return _level; } 
        set { SetLevel(value); }
    }

    public void SetLevel(double level) {
        _level = level;
        if (_level <= QuaterLevel) { //0-25%
            var c = Color.black;
            c.a = (float)(_level / QuaterLevel);
            Material.color = c;
            Material.SetColor("_EmissionColor",c);
        } else if(_level <= HalfLevel) { //25-50%
            var t = (_level - QuaterLevel)/HalfLevel;
            var c = Color.Lerp(Color.black, _color, (float) t);
            Material.color = c;
            c = Color.Lerp(Color.black, _emission, (float) t);
            Material.SetColor("_EmissionColor", c);
        } else { //50-100%
            var t = (_level - ThreeQuaterLevel)/QuaterLevel; //from -1 to 1
            var c = Material.color;
            c.a = (float)(1 - t);
            Material.color = c;
        }
    }
}
