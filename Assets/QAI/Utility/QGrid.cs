using System;
using UnityEngine;

public class QGrid {
    /// <summary>
    /// Length of the X axis
    /// </summary>
    public int Width { get; private set; }
    /// <summary>
    /// Length of the Y axis
    /// </summary>
    public int Height { get; private set; }
    /// <summary>
    /// Length of the Z axis
    /// </summary>
    public int Depth { get; private set; }
    public float ResolutionX { get; private set; }
    public float ResolutionY { get; private set; }
    public float ResolutionZ { get; private set; }
    public Transform Transform { get; private set; }
    public Vector3 Offset { get; private set; }
    public Vector3 Center { get { return Transform.position + Offset; } }
    public Vector3 Size { get; private set; }
    public Bounds Bounds { get { return new Bounds(Center, Size);}}



    public readonly double[] Grid;


    public double this[int x, int y, int z] {
        get { return Grid[x + Width * (y + Height * z)]; }
        set { Grid[x + Width * (y + Height * z)] = value; }
    }

    public double this[Coordinates c] {
        get { return this[c.X, c.Y, c.Z]; }
        set { this[c.X, c.Y, c.Z] = value; }
    }

    public QGrid(int width, int height, int depth, Transform transform, float resolution) {
        Init(width, height, depth, transform, Vector3.zero, resolution, resolution, resolution);
        Grid = new double[Width * Height * Depth];
    }

    public QGrid(int width, int height, int depth, Transform transform, Vector3 offset, float resolution) {
        Init(width, height, depth, transform, offset, resolution, resolution, resolution);
        Grid = new double[Width * Height * Depth];
    }

    public QGrid(int width, int height, int depth, Transform transform, Vector3 offset, float resolutionX, float resolutionY, float resolutionZ) {
        Init(width, height, depth, transform, offset, resolutionX, resolutionY, resolutionZ);
        Grid = new double[Width * Height * Depth];
    }

    private void Init(int width, int height, int depth, Transform transform, Vector3 offset, float resolutionX, float resolutionY, float resolutionZ) {
        Width = width;
        Height = height;
        Depth = depth;
        ResolutionX = resolutionX;
        ResolutionY = resolutionY;
        ResolutionZ = resolutionZ;
        Transform = transform;
        Offset = offset;
        Size = new Vector3(Width * ResolutionX, Height * ResolutionY, Depth * ResolutionZ);
    }

    /// <summary>
    /// Locate a point's position in the grid
    /// </summary>
    /// <param name="p">The point, which position will be located in the grid</param>
    /// <returns>If the point is within the bounds of the grid the coordinates to the grid cell where in the point is located will be returned</returns>
    public Coordinates? Locate(Vector3 p) {
        if (Bounds.Contains(p)) {
            var d = p - Center;
            d = new Vector3(d.x/ResolutionX, d.y/ResolutionY, d.z/ResolutionZ);
            var c = new Vector3((int)(Width/2f), (int)(Height/2f), (int)(Depth/2f));
            var r = c + d;
            return new Coordinates(Mathf.RoundToInt(r.x), Mathf.RoundToInt(r.y), Mathf.RoundToInt(r.z));
        }
        return null;
    }

    public void Populate(Func<Bounds, double> populator) {
        Iterate((c, b) => this[c] = populator(b));
    }

    public void DebugDraw(Func<double, Color> colorFunc = null) {
        Iterate((coor, b) => {
            var c = b.center;
            const float skin = 0.02f; //in order to avoid drawing over the previous cell wall
            var rX = ResolutionX/2 - skin; 
            var rY = ResolutionY/2 - skin; 
            var rZ = ResolutionZ/2 - skin;
            var aaa = new Vector3(c.x - rX, c.y - rY, c.z - rZ);
            var baa = new Vector3(c.x + rX, c.y - rY, c.z - rZ);
            var aba = new Vector3(c.x - rX, c.y + rY, c.z - rZ);
            var bba = new Vector3(c.x + rX, c.y + rY, c.z - rZ);
            var aab = new Vector3(c.x - rX, c.y - rY, c.z + rZ);
            var bab = new Vector3(c.x + rX, c.y - rY, c.z + rZ);
            var abb = new Vector3(c.x - rX, c.y + rY, c.z + rZ);
            var bbb = new Vector3(c.x + rX, c.y + rY, c.z + rZ);
            var color = colorFunc == null ? Color.white : colorFunc(this[coor]);

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


    private void Iterate(Action<Coordinates,Bounds> f) {
        var center = Center;
        var left = center.x - ((Width/2f)*ResolutionX - ResolutionX/2);
        var top = center.y - ((Height/2f)*ResolutionY - ResolutionY/2);
        var front = center.z -((Depth/2f)*ResolutionZ - ResolutionZ/2);
        var cellsize = new Vector3(ResolutionX, ResolutionY, ResolutionZ);
        for (var x = 0; x < Width; x++) {
            for (var y = 0; y < Height; y++) {
                for (var z = 0; z < Depth; z++) {
                    var c = new Vector3(left + x * ResolutionX, top + y * ResolutionY, front + z * ResolutionZ);
                    f(new Coordinates(x,y,z), new Bounds(c, cellsize));
                }
            }
        }
    }
}