using System;
using UnityEngine;

public class QGrid {
    public int Width { get; private set; } //x
    public int Height { get; private set; } //y
    public float ResolutionX { get; private set; }
    public float ResolutionY { get; private set; }

    public readonly double[] Grid;

    public double this[int x, int y] {
        get {
            return Grid[x*Width + y];
        }
        set {
            Grid[x*Width + y] = value;
        }
    }

    public QGrid(int height, int width, float resolution) {
        Init(height,width,resolution,resolution);
        Grid = new double[Width * Height];
    }

    public QGrid(int height, int width, float resolutionX, float resolutionY) {
        Init(height,width,resolutionX,resolutionY);
        Grid = new double[Width * Height];
    }

    private void Init(int height, int width, float resolutionX, float resolutionY) {
        Height = height;
        Width = width;
        ResolutionX = resolutionX;
        ResolutionY = resolutionY;
    }

    public void Populate(Vector2 center, Func<Vector2, double> populator) {
        Iterate(center, (x, y, p) => this[x,y] = populator(p));
    }

    public void DebugDraw(Vector2 center, Func<double, Color> colorFunc = null) {
        Iterate(center, (x, y, p) => {
            var nw = new Vector3(p.x - ResolutionX/2, p.y - ResolutionY/2);
            var ne = new Vector3(p.x + ResolutionX/2, p.y - ResolutionY/2);
            var sw = new Vector3(p.x - ResolutionX/2, p.y + ResolutionY/2);
            var se = new Vector3(p.x + ResolutionX/2, p.y + ResolutionY/2);
            var c = colorFunc == null ? Color.white : colorFunc(this[x,y]);
            Debug.DrawLine(nw, ne, c);
            Debug.DrawLine(ne, se, c);
            Debug.DrawLine(se, sw, c);
            Debug.DrawLine(sw, nw, c);
        });
    }


    private void Iterate(Vector2 center, Action<int,int,Vector2> f) {
        var left = center.x - ((Width/2f)*ResolutionX - ResolutionX/2);
        var top = center.y - ((Height/2f)*ResolutionY - ResolutionY/2);
        for (var x = 0; x < Width; x++) {
            for (var y = 0; y < Height; y++) {
                f(x, y, new Vector2(left + x * ResolutionX, top + y * ResolutionY));
            }
        }
    }
}
