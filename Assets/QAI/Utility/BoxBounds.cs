using UnityEngine;
using System.Collections;

public struct BoxBounds {
    private Bounds _bounds;
    public readonly Quaternion Rotation;
    private readonly Quaternion _inverseRotation;

    public BoxBounds(Vector3 center, Vector3 size, Quaternion rotation) {
        _bounds = new Bounds(center, size);
        Rotation = rotation;
        _inverseRotation = Quaternion.Inverse(Rotation);
    }

    public Vector3 center { get { return _bounds.center; } set { _bounds.center = value; } }

    public Vector3 size { get { return _bounds.size; } set { _bounds.size = value; } }

    public Vector3[] CornersXY() {
        var c = _bounds.center;
        var s = _bounds.extents;
        return new[] {
            Rotate(new Vector3(c.x + s.x, c.y + s.y, 0)),
            Rotate(new Vector3(c.x + s.x, c.y - s.y, 0)),
            Rotate(new Vector3(c.x - s.x, c.y - s.y, 0)),
            Rotate(new Vector3(c.x - s.x, c.y + s.y, 0))
        };
    }
    public Vector3[] CornersXZ() {
        var c = _bounds.center;
        var s = _bounds.extents;
        return new[] {
            Rotate(new Vector3(c.x + s.x, 0, c.z + s.z)),
            Rotate(new Vector3(c.x + s.x, 0, c.z - s.z)),
            Rotate(new Vector3(c.x - s.x, 0, c.z - s.z)),
            Rotate(new Vector3(c.x - s.x, 0, c.z + s.z))
        };
    }

    public void DebugDraw(Vector3[] corners, Color color) {
        Debug.DrawLine(corners[0], corners[1], color); //down
        Debug.DrawLine(corners[1], corners[2], color); //left
        Debug.DrawLine(corners[2], corners[3], color); //up
        Debug.DrawLine(corners[3], corners[0], color); //right
    }

    public void DebugDrawXY(Color color) {
        DebugDraw(CornersXY(), color);
    }
    public void DebugDrawXZ(Color color) {
        DebugDraw(CornersXZ(), color);
    }

    public bool Contains(Vector3 point) {
        return _bounds.Contains(RotateInverse(point));
    }

    private Vector3 Rotate(Vector3 point) {
        return point.RotatePoint(_bounds.center, Rotation);
    }
    private Vector3 RotateInverse(Vector3 point) {
        return point.RotatePoint(_bounds.center, _inverseRotation);
    }

}
