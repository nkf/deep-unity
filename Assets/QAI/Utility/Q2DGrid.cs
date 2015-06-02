using System;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

public class Q2DGrid {
    public int GridSize { get; private set; }
    public Transform Transform { get; private set; }
    public readonly float ResolutionX, ResolutionY, ResolutionZ;
    public Vector3 Offset { get; private set; }
    public Vector3 Center { get { return Transform.position + Offset; } }
    public Vector3 Size { get; private set; }
    public Bounds Bounds { get { return new Bounds(Center, Size); } }
    public Axis NormalAxis { get; private set; }
    public Matrix<float> Matrix { get; private set; }

    public Q2DGrid(int size, Transform transform, GridSettings gs = null) {
        if(gs == null) gs = new GridSettings();
        Matrix = Matrix<float>.Build.Dense(size, size);
        GridSize = size;
        Transform = transform;
        Offset = gs.Offset;
        Size = new Vector3(size * gs.ResolutionX, size * gs.ResolutionY, size * gs.ResolutionZ);
        NormalAxis = gs.NormalAxis;
        ResolutionX = gs.ResolutionX;
        ResolutionY = gs.ResolutionY;
        ResolutionZ = gs.ResolutionZ;
    }

    public float this[int x, int y] {
        get { return Matrix[x, y]; }
        set { Matrix[x, y] = value; }
    }

    public float this[Coordinates2D c] {
        get { return Matrix[c.X, c.Y]; }
        set { Matrix[c.X, c.Y] = value; }
    }

    public void Populate(Func<Bounds, float> populator) {
        Iterate((c, b) => this[c] = populator(b));
    }

    /// <summary>
    /// Locate a point's position in the grid
    /// </summary>
    /// <param name="p">The point, which position will be located in the grid</param>
    /// <returns>If the point is within the bounds of the grid the coordinates to the grid cell where in the point is located will be returned</returns>
    public Coordinates2D? Locate(Vector3 p) {
        var b = Bounds;
        //Since lower bound is exclusive we subtract a small amount to make the upper bound exclusive aswell. 
        b.size -= new Vector3(0.01f, 0.01f, 0.01f);
        if(b.Contains(p)) {
            var d = p - Center;
            d = new Vector3(d.x / ResolutionX, d.y / ResolutionY, d.z / ResolutionZ);
            var halfSize = (GridSize - 1)/2f;
            var c = new Vector3(halfSize,halfSize,halfSize);
            var r = c + d;
            if(NormalAxis == Axis.Y) return new Coordinates2D(Mathf.RoundToInt(r.x), Mathf.RoundToInt(r.z));
            if(NormalAxis == Axis.Z) return new Coordinates2D(Mathf.RoundToInt(r.x), Mathf.RoundToInt(r.y));
        }
        return null;
    }

    public void DebugDraw(Func<float, Color> colorFunc = null) {
        const float skin = 0.01f; //in order to avoid drawing over the previous cell wall
        var rX = ResolutionX / 2 - skin;
        var rY = ResolutionY / 2 - skin;
        var rZ = ResolutionZ / 2 - skin;
        Iterate((coor, b) => {
            var c = b.center;
            var color = colorFunc == null ? Color.white : colorFunc(this[coor]);
            var points = NormalAxis == Axis.Z ? DebugDrawXY(c, rX, rY) : DebugDrawXZ(c, rX, rZ);
            Debug.DrawLine(points[0], points[1], color); //down
            Debug.DrawLine(points[1], points[2], color); //left
            Debug.DrawLine(points[2], points[3], color); //up
            Debug.DrawLine(points[3], points[0], color); //right
        });
    }

    private Vector3[] DebugDrawXY(Vector3 c, float rX, float rY) {
        return new[] {
            new Vector3(c.x + rX, c.y + rY, 0),
            new Vector3(c.x + rX, c.y - rY, 0),
            new Vector3(c.x - rX, c.y - rY, 0),
            new Vector3(c.x - rX, c.y + rY, 0)
        };
    }
    private Vector3[] DebugDrawXZ(Vector3 c, float rX, float rZ) {
        return new[] {
            new Vector3(c.x + rX, 0, c.z + rZ),
            new Vector3(c.x + rX, 0, c.z - rZ),
            new Vector3(c.x - rX, 0, c.z - rZ),
            new Vector3(c.x - rX, 0, c.z + rZ)
        };
    }

    private void Iterate(Action<Coordinates2D, Bounds> f) {
        var left = Center.x -((GridSize/2f)*ResolutionX - ResolutionX/2);
        var resolutionUp = NormalAxis == Axis.Z ? ResolutionY : ResolutionZ;
        var center = NormalAxis == Axis.Z ? Center.y : Center.z;
        var top = center - ((GridSize/2f)*resolutionUp - resolutionUp/2);
        var cellsize = new Vector3(ResolutionX, ResolutionY, ResolutionZ);
        var c = Vector3.zero;
        for(var x = 0; x < GridSize; x++) {
            for(var y = 0; y < GridSize; y++) {
                if(NormalAxis == Axis.Z)
                    c = new Vector3(left + x * ResolutionX, top + y * resolutionUp, 0);
                if(NormalAxis == Axis.Y)
                    c = new Vector3(left + x * ResolutionX, 0, top + y * resolutionUp);
                f(new Coordinates2D(x, y), new Bounds(c, cellsize));
            }
        }
    }
}

public struct Coordinates2D {
    public readonly int X, Y;
    public Coordinates2D(int x, int y) { X = x; Y = y; }
    public bool Equals(Coordinates2D other) {
        return X == other.X && Y == other.Y;
    }
    public override bool Equals(object obj) {
        if(ReferenceEquals(null, obj)) return false;
        return obj is Coordinates2D && Equals((Coordinates2D)obj);
    }
    public override int GetHashCode() {
        unchecked { return (X * 397) ^ Y; }
    }
    public override string ToString() {
        return string.Format("[{0},{1}]", X, Y);
    }
}

public enum Axis {
    Z, Y
}

public class GridSettings {
    public Vector3 Offset;
    public Axis NormalAxis;
    public float ResolutionX, ResolutionY, ResolutionZ;
    public GridSettings() {
        //Do not set the value for normal axis in the constructor (or at the declaration) because it bugs the object initializer, by always overriding it.
        //By not setting the value it will default to the first declared value in the Axis enum.
        Offset = Vector3.zero;
        ResolutionX = 1;
        ResolutionY = 1;
        ResolutionZ = 1;
    }
}
