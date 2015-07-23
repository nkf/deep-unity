using UnityEngine;
using System.Collections;

public class CircleCalculator {

    public Vector2 CalculateCircleCenter(Vector2 p1, Vector2 p2, Vector2 p3) {
        Vector2 center = new Vector2();
        var ma = (p2.y - p1.y) / (p2.x - p1.x);
        var mb = (p3.y - p2.y) / (p3.x - p2.x);
        center.x = (ma * mb * (p1.y - p3.y) + mb * (p1.x - p2.x) - ma * (p2.x + p3.x)) / (2 * (mb - ma));
        center.y = (-1 / ma) * (center.x - (p1.x + p2.x) / 2) + (p1.y + p2.y) / 2;
        return center;
    }

    public float CalculateAngle(Vector2 p1, Vector2 p2, Vector2 p3) {
        var center = CalculateCircleCenter(p1, p2, p3);
        var r = (center - p1).magnitude;
        return 2*Mathf.Acos((0.5f*(p1 - p3).magnitude)/r); 
    }
}
