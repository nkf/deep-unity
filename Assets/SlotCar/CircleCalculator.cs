using UnityEngine;
using System.Collections;

public class CircleCalculator {
	public enum Direction { Straight, Right, Left }
    public static Vector2? CalculateCircleCenter(Vector2 p1, Vector2 p2, Vector2 p3, out Direction dir) {
		float t = p2.x*p2.x+p2.y*p2.y;
		float bc = (p1.x*p1.x + p1.y*p1.y - t)/2.0f;
		float cd = (t - p3.x*p3.x - p3.y*p3.y)/2.0f;
		float det = (p1.x-p2.x)*(p2.y-p3.y)-(p2.x-p3.x)*(p1.y-p2.y);
		
		if (Mathf.Abs(det) > 1.0e-6) {
			dir = det > 0 ? Direction.Left : Direction.Right;
			det = 1/det;
			float x = (bc*(p2.y - p3.y) - cd*(p1.y - p2.y))*det;
			float y = ((p1.x - p2.x)*cd - (p2.x - p3.x)*bc)*det;			
			return new Vector2(x, y);
		}
		dir = Direction.Straight;
        return null;
    }

    public static float CalculateAngle(Vector2 p1, Vector2 p2, Vector2 p3) {
		var dir = Direction.Straight;
        var center = CalculateCircleCenter(p1, p2, p3, out dir).Value;
        var r = (center - p1).magnitude;
        return 2*Mathf.Acos((0.5f*(p1 - p3).magnitude)/r); 
    }

	public static float CalculateCurvature(Vector2 p1, Vector2 p2, Vector2 p3) {
		var dir = Direction.Straight;
		var center = CalculateCircleCenter(p1, p2, p3, out dir);
		if(!center.HasValue)
			return 0;
		var r = (center.Value - p1).magnitude;
		return 1f/r;
	}
}
