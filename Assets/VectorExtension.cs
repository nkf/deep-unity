using UnityEngine;

public static class VectorExtension {
    public static Vector3 RotatePoint(this Vector3 point, Vector3 pivot, Quaternion rotation) {
        var dir = point - pivot; // get point direction relative to pivot
        dir = rotation * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point;
    }
}
