using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve
{
   
	public Vector3[] points;

	public void Reset () {
		points = new Vector3[] {
			new Vector3(1f, 0f, 0f),
			new Vector3(2f, 0f, 0f),
			new Vector3(3f, 0f, 0f)
		};
	}

	public void SetPoints(Vector3 p0, Vector3 p1, Vector3 p2) {
		if (points == null)
			points = new Vector3[3];

		points[0] = p0;
		points[1] = p1;
		points[2] = p2;
    }
	

	public Vector3 GetPoint (float t) {
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return
			oneMinusT * oneMinusT * points[0] +
			2f * oneMinusT * t * points[1] +
			t * t * points[2];
	}
}
