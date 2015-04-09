using System;
using UnityEngine;

public class QGrid {
    public int Width { get; private set; } //x
    public int Height { get; private set; } //y
    public int Depth { get; private set; } //z
    public float ResolutionX { get; private set; }
    public float ResolutionY { get; private set; }
    public float ResolutionZ { get; private set; }

    public readonly double[] Grid;

    public double this[int x, int y, int z] {
        get {
            return Grid[x + Width * (y + Height * z)];
        }
        set {
            Grid[x + Width * (y + Height * z)] = value;
        }
    }

    public QGrid(int height, int width, int depth, float resolution) {
        Init(height, width, depth,resolution, resolution, resolution);
        Grid = new double[Width * Height * Depth];
    }

    public QGrid(int height, int width, int depth, float resolutionX, float resolutionY, float resolutionZ) {
        Init(height,width,depth,resolutionX,resolutionY, resolutionZ);
        Grid = new double[Width * Height * Depth];
    }

    private void Init(int height, int width, int depth, float resolutionX, float resolutionY, float resolutionZ) {
        Height = height;
        Width = width;
        Depth = depth;
        ResolutionX = resolutionX;
        ResolutionY = resolutionY;
        ResolutionZ = resolutionZ;
    }

    public void Populate(Vector3 center, Func<Bounds, double> populator) {
        Iterate(center, (x, y, z, p) => this[x,y,z] = populator(p));
    }

    public void DebugDraw(Vector3 center, Func<double, Color> colorFunc = null) {
        Iterate(center, (x, y, z, b) => {
            var c = b.center;
            var rX = ResolutionX/2; var rY = ResolutionY/2; var rZ = ResolutionZ/2;
            var aaa = new Vector3(c.x - rX, c.y - rY, c.z - rZ);
            var baa = new Vector3(c.x + rX, c.y - rY, c.z - rZ);
            var aba = new Vector3(c.x - rX, c.y + rY, c.z - rZ);
            var bba = new Vector3(c.x + rX, c.y + rY, c.z - rZ);
            var aab = new Vector3(c.x - rX, c.y - rY, c.z + rZ);
            var bab = new Vector3(c.x + rX, c.y - rY, c.z + rZ);
            var abb = new Vector3(c.x - rX, c.y + rY, c.z + rZ);
            var bbb = new Vector3(c.x + rX, c.y + rY, c.z + rZ);
            var color = colorFunc == null ? Color.white : colorFunc(this[x,y,z]);

            //The first four
            Debug.DrawLine(aaa, baa, color);
            Debug.DrawLine(baa, bba, color);
            Debug.DrawLine(bba, aba, color);
            Debug.DrawLine(aba, aaa, color);

            //The second four
            Debug.DrawLine(aab, bab, color);
            Debug.DrawLine(bab, bbb, color);
            Debug.DrawLine(bbb, abb, color);
            Debug.DrawLine(abb, aab, color);

            //The connection between the two layers
            Debug.DrawLine(aaa, aab, color);
            Debug.DrawLine(baa, bab, color);
            Debug.DrawLine(aba, abb, color);
            Debug.DrawLine(bba, bbb, color);
        });
    }


    private void Iterate(Vector3 center, Action<int,int,int,Bounds> f) {
        var left = center.x - ((Width/2f)*ResolutionX - ResolutionX/2);
        var top = center.y - ((Height/2f)*ResolutionY - ResolutionY/2);
        var front = center.z -((Depth/2f)*ResolutionZ - ResolutionZ/2);
        var size = new Vector3(ResolutionX, ResolutionY, ResolutionZ);
        for (var x = 0; x < Width; x++) {
            for (var y = 0; y < Height; y++) {
                for (var z = 0; z < Depth; z++) {
                    var c = new Vector3(left + x * ResolutionX, top + y * ResolutionY, front + z * ResolutionZ);
                    f(x, y, z, new Bounds(c, size));
                }
            }
        }
    }
}
