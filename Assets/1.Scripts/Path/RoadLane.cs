using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoadLane : Lane<RoadLaneData> {

	[SerializeField] private List<AICar> aiCars = new();
	[SerializeField] private List<NextRoadLane> nextRoadLanes = new();

	public Vector3 StartPos { get; private set; }
	public Vector3 EndPos { get; private set; }

	public bool AllowPassing = true;
	
	protected override void OnInit() {
		
	}
	
	public bool HasNextRoadLanes() {
		return nextRoadLanes.Count > 0;
	}

	public void SetStartPosAndEndPos() {
		StartPos = transform.position + transform.right * Settings.Instance.laneSize / 2f;
		EndPos = StartPos + transform.forward * Length;
		if (!Data.hasFrontDirection) {
			(StartPos, EndPos) = (EndPos, StartPos);
		}
	}

	public void SpawnAICars(Transform parent) {

		int randomSpawnDistance() {
			return Random.Range(Settings.Instance.spawnAICarDistanceMin, Settings.Instance.spawnAICarDistanceMax);
		}
		
		Vector3 dir = (EndPos - StartPos).normalized;

		for (int z = randomSpawnDistance(); z < Length - 4f; z += randomSpawnDistance()) {
			AICar carPrefab = Settings.Instance.aiCarPrefabs[Random.Range(0, Settings.Instance.aiCarPrefabs.Length)];
			AICar aiCar = ObjectPoolManager.Get(carPrefab, parent);
			aiCar.name = carPrefab.name;

			Vector3 pos = StartPos + dir * z;
			aiCar.transform.position = new Vector3(pos.x, carPrefab.transform.position.y, pos.z);
			aiCar.transform.LookAt(new Vector3(EndPos.x, aiCar.transform.position.y, EndPos.z));
			
			aiCar.SetTargetPoint(new TargetPoint {
				pos = EndPos,
				onReach = OnAICarReachTargetPoint,
				pass = false
			});
			aiCars.Add(aiCar);
			
			aiCar.gameObject.SetActive(true);
		}
	}

	private void Transition(List<Vector3> points, AICar aiCar) {
		
		List<TargetPoint> targetPointsTransition = new(points.Count);
		for (int i = 0; i < points.Count; i++) {
			targetPointsTransition.Add(new TargetPoint {
				pos = points[i],
				minDistToReach = 1f
			});
		}

		TravelAICarTargetPoints(aiCar, targetPointsTransition, () => {
			aiCar.SetTargetPoint(new TargetPoint {
				pos = EndPos,
				onReach = OnAICarReachTargetPoint
			});
			aiCars.Add(aiCar);
		});
	}

	private static void TravelAICarTargetPoints(AICar aiCar, List<TargetPoint> points, Action onComplete) {
		if (points.Count == 0) {
			onComplete?.Invoke();
			return;
		}
		points[0].onReach = _ => {
			points.RemoveAt(0);
			TravelAICarTargetPoints(aiCar, points, onComplete);
		};
		aiCar.SetTargetPoint(points[0]);
	}
	
	private void OnAICarReachTargetPoint(AICar aiCar) {
		aiCars.Remove(aiCar);
		if (nextRoadLanes.Count == 0) {
			ObjectPoolManager.Release(aiCar);
		} else {
			NextRoadLane nextRoadLane = nextRoadLanes[Random.Range(0, nextRoadLanes.Count)];
			nextRoadLane.roadLane.Transition(nextRoadLane.transPoints, aiCar);	
		}
	}

	public void AddNextRoadLane(RoadLane nextRoadLane, List<Vector3> transPoints) {
		nextRoadLanes.Add(new NextRoadLane {
			roadLane = nextRoadLane,
			transPoints = transPoints
		});
	}

	public override void Clear() {
		base.Clear();
		foreach (AICar aiCar in aiCars) {
			ObjectPoolManager.Release(aiCar);
		}
		aiCars.Clear();
	}

	/*private List<Vector3> GetTransitionPoints(Vector3 point0, Vector3 point1) {
		Vector3 dir0 = transform.forward.normalized;
		Vector3 vectorToPoint1 = point1 - point0;
		Vector3 projection = Vector3.Dot(vectorToPoint1, dir0) * dir0;
		Vector3 perpendicularVector = vectorToPoint1 - projection;
		Vector3 perpendicularPoint = point0 + perpendicularVector;
		return Chaikin.SmoothPath(new List<Vector3> { point0, perpendicularPoint, point1 }, 2);
	}*/

	private void OnDrawGizmos() {
		
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(StartPos, EndPos);
		
		for (int i = 0; i < nextRoadLanes.Count; i++) {
			List<Vector3> points = nextRoadLanes[i].transPoints;
			
			Gizmos.color = Color.blue;
			for (int j = 0; j < points.Count - 1; j++) {
				Gizmos.DrawLine(points[j], points[j + 1]);
			}
			
			Gizmos.color = Color.yellow;
			for (int j = 0; j < points.Count; j++) {
				Gizmos.DrawSphere(points[j],0.1f);
			}
		}
	}
	
	[Serializable]
	private class NextRoadLane {
		public RoadLane roadLane;
		public List<Vector3> transPoints;
	}
}

public class RoadLaneData : LaneData {
	public bool hasFrontDirection;
}