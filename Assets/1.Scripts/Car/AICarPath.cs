using System;
using System.Collections.Generic;
using UnityEngine;

public class AICarPath : MonoBehaviour {

	[SerializeField] private List<Transform> waypoints;
	[SerializeField] private List<Vector3> points;
	[SerializeField] private uint pointsCount = 100;

	private AICar car;
	private Action onComplete;
	private Func<bool> canPass;
	private int currentPointIndex;
	
	public void SetCar(AICar car, Action onComplete, Func<bool> canPass) {
		this.car = car;
		this.onComplete = onComplete;
		this.canPass = canPass;
		SetTargetPoint();
	}
	
	public float GetProgress() {
		return (float)currentPointIndex / pointsCount;
	}

	private void SetTargetPoint(int index = 0) {
		currentPointIndex = index;
		if (index >= points.Count) {
			onComplete?.Invoke();
			return;
		}
		car.SetTargetPoint(new TargetPoint {
			pos = points[index],
			pass = true,
			allowPassing = () => canPass(),
			minDistToReach = 1f,
			onReach = _ => {
				SetTargetPoint(index + 1);
			}
		});
	}

	private void CreatePoints() {
		List<Vector3> controlPoints = new();
		foreach (var waypoint in waypoints) {
			controlPoints.Add(waypoint.transform.position);
		}
		points = Bezier.GetBezierPoints(controlPoints, pointsCount);
	}

	[Space]
	[SerializeField] protected float gizmosSize = 0.1f;
	[SerializeField] private bool drawGizmos;
	private void OnDrawGizmos() {
		if (!drawGizmos) {
			return;
		}
		if (!Application.isPlaying) {
			if (waypoints.Count != transform.childCount) {
				waypoints = new List<Transform>();
				for (int i = 0; i < transform.childCount; i++) {
					waypoints.Add(transform.GetChild(i));
					waypoints[i].transform.name = $"Waypoint{i}";
				}
			}
			CreatePoints();
		}
		Gizmos.color = Color.red;
		for (int i = 0; i < waypoints.Count; i++) {
			Gizmos.DrawSphere(waypoints[i].transform.position, gizmosSize);
		}
		Gizmos.DrawLine(transform.position, points[0]);
		Gizmos.color = Color.yellow;
		for (int i = 0; i < points.Count - 1; i++) {
			Gizmos.DrawLine(points[i], points[i + 1]);
		}
	}
}