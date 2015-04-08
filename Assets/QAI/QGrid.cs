
using System;
using System.Linq;
using UnityEngine;

public class QGrid {
    public int Width { get; private set; } //x
    public int Height { get; private set; } //y
    public float ResolutionX { get; private set; }
    public float ResolutionY { get; private set; }

    public readonly double[] Grid;

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
        var left = center.x - ((Width/2f) * ResolutionX);
        var top = center.y - ((Height/2f) * ResolutionY);
        for (var x = 0; x < Width; x++) {
            for (var y = 0; y < Height; y++) {
                Grid[x * Width + y] = populator( new Vector2(left + x * ResolutionX, top + y * ResolutionY) );
            }
        }
    }


}
