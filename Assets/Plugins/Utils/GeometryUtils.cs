using System.Collections.Generic;
using UnityEngine;

public static class GeometryUtils {

	public static float DistancePointLine2D(Vector2 point, Vector2 lineStart, Vector2 lineEnd) {
		return (ProjectPointLine2D(point, lineStart, lineEnd) - point).magnitude;
	}

	public static Vector2 ProjectPointLine2D(Vector2 point, Vector2 lineStart, Vector2 lineEnd) {
		Vector2 rhs = point - lineStart;
		Vector2 vector2 = lineEnd - lineStart;
		float magnitude = vector2.magnitude;
		Vector2 lhs = vector2;
		if (magnitude > 1E-06f) {
			lhs = (Vector2)(lhs / magnitude);
		}
		float num2 = Mathf.Clamp(Vector2.Dot(lhs, rhs), 0f, magnitude);
		return (lineStart + ((Vector2)(lhs * num2)));
	}

	public static float ClosestDistanceToPolygon(Vector2[] verts, Vector2 point) {
		int nvert = verts.Length;
		int i, j = 0;
		float minDistance = Mathf.Infinity;
		for (i = 0, j = nvert - 1; i < nvert; j = i++) {
			float distance = DistancePointLine2D(point, verts[i], verts[j]);
			minDistance = Mathf.Min(minDistance, distance);
		}
		return minDistance;
	}

	public static bool IsInsidePolygon(Vector2[] vertices, Vector2 checkPoint, float margin = 0.01f) {
		if (ClosestDistanceToPolygon(vertices, checkPoint) < margin) {
			return true;
		}
		float[] vertX = new float[vertices.Length];
		float[] vertY = new float[vertices.Length];
		for (int i = 0; i < vertices.Length; i++) {
			vertX[i] = vertices[i].x;
			vertY[i] = vertices[i].y;
		}
		return IsInsidePolygon(vertices.Length, vertX, vertY, checkPoint.x, checkPoint.y);
	}

	public static bool IsInsidePolygon(int nvert, float[] vertx, float[] verty, float testx, float testy) {
		bool c = false;
		int i, j = 0;
		for (i = 0, j = nvert - 1; i < nvert; j = i++) {
			if ((((verty[i] <= testy) && (testy < verty[j])) ||
			     ((verty[j] <= testy) && (testy < verty[i]))) &&
			    (testx < (vertx[j] - vertx[i]) * (testy - verty[i]) / (verty[j] - verty[i]) + vertx[i]))
				c = !c;
		}
		return c;
	}

	public static bool PointInPolygon(Vector3 testPoint, List<Vector3> vertices) {
		// Sanity check - not enough bounds vertices = nothing to be inside of
		if (vertices.Count < 3)
			return false;

		// Check how many lines this test point collides with going in one direction
		// Odd = Inside, Even = Outside
		var collisions = 0;
		var vertexCounter = 0;
		var startPoint = vertices[vertices.Count - 1];

		// We recenter the test point around the origin to simplify the math a bit
		startPoint.x -= testPoint.x;
		startPoint.z -= testPoint.z;
		var currentSide = false;
		if (!ApproximatelyZero(startPoint.z)) {
			currentSide = startPoint.z < 0f;
		} else {
			// We need a definitive side of the horizontal axis to start with (since we need to know when we
			// cross it), so we go backwards through the vertices until we find one that does not lie on the horizontal
			for (var i = vertices.Count - 2; i >= 0; --i) {
				var vertZ = vertices[i].z;
				vertZ -= testPoint.z;
				if (!ApproximatelyZero(vertZ)) {
					currentSide = vertZ < 0f;
					break;
				}
			}
		}
		while (vertexCounter < vertices.Count) {
			var endPoint = vertices[vertexCounter];
			endPoint.x -= testPoint.x;
			endPoint.z -= testPoint.z;
			var startToEnd = endPoint - startPoint;
			var edgeSqrMagnitude = startToEnd.sqrMagnitude;
			if (ApproximatelyZero(startToEnd.x * endPoint.z - startToEnd.z * endPoint.x) &&
			    startPoint.sqrMagnitude <= edgeSqrMagnitude && endPoint.sqrMagnitude <= edgeSqrMagnitude) {
				// This line goes through the start point, which means the point is on an edge of the polygon
				return true;
			}

			// Ignore lines that end at the horizontal axis
			if (!ApproximatelyZero(endPoint.z)) {
				var nextSide = endPoint.z < 0f;
				if (nextSide != currentSide) {
					currentSide = nextSide;

					// If we've crossed the horizontal, check if the origin is to the left of the line
					if ((startPoint.x * endPoint.z - startPoint.z * endPoint.x) / -(startPoint.z - endPoint.z) > 0)
						collisions++;
				}
			}
			startPoint = endPoint;
			vertexCounter++;
		}
		return collisions % 2 > 0;
	}

	private static bool ApproximatelyZero(float value) {
		return Mathf.Abs(value) < Mathf.Epsilon;
	}
}