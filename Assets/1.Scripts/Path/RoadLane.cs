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

	public void ClearNextRoadLanes() {
		nextRoadLanes.Clear();
		AllowPassing = true;
	}

	public void SpawnAICars() {
		int randomSpawnDistance() {
			return Random.Range(Settings.Instance.spawnAICarDistanceMin, Settings.Instance.spawnAICarDistanceMax);
		}
		for (int z = randomSpawnDistance(); z < Length - 4f; z += randomSpawnDistance()) {
			SpawnAICar(z);
		}
	}

	private void SpawnAICar(float posZ, bool checkAround = false) {
		AICar carPrefab = Settings.Instance.aiCarPrefabs[Random.Range(0, Settings.Instance.aiCarPrefabs.Length)];
		Vector3 dir = (EndPos - StartPos).normalized;
		Vector3 pos = StartPos + dir * posZ;
		pos.y = carPrefab.transform.position.y;

		if (checkAround) {
			for (int i = 0; i < aiCars.Count; i++) {
				if (Vector3.Distance(aiCars[i].transform.position, pos) < 10f) {
					this.Wait(1f, () => SpawnAICar(posZ, checkAround));
					return;
				}
			}
			if (Vector3.Distance(PathController.Instance.UserCar.transform.position, pos) < 80f) {
				this.Wait(1f, () => SpawnAICar(posZ, checkAround));
				return;
			}
		}
		
		AICar aiCar = ObjectPoolManager.Get(carPrefab, PathController.Instance.transform);
		aiCar.name = carPrefab.name;
		aiCar.transform.position = pos;
		aiCar.transform.LookAt(new Vector3(EndPos.x, aiCar.transform.position.y, EndPos.z));
			
		aiCar.SetTargetPoint(new TargetPoint {
			pos = EndPos,
			onReach = OnAICarReachTargetPoint,
			allowPassing = () => AllowPassing
		});
		aiCars.Add(aiCar);
			
		aiCar.gameObject.SetActive(true);
	}

	private void Transition(List<Vector3> points, AICar aiCar) {
		
		List<TargetPoint> targetPointsTransition = new(points.Count);
		for (int i = 0; i < points.Count; i++) {
			targetPointsTransition.Add(new TargetPoint {
				pos = points[i],
				minDistToReach = 2f
			});
		}
		
		aiCars.Add(aiCar);
		TravelAICarTargetPoints(aiCar, targetPointsTransition, () => {
			aiCar.SetTargetPoint(new TargetPoint {
				pos = EndPos,
				onReach = OnAICarReachTargetPoint,
				allowPassing = () => AllowPassing
			});
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
		if (nextRoadLanes.Count == 0) {
			aiCars.Remove(aiCar);
			ObjectPoolManager.Release(aiCar);
		} else {
			aiCars.Remove(aiCar);
			NextRoadLane nextRoadLane = nextRoadLanes[Random.Range(0, nextRoadLanes.Count)];
			nextRoadLane.roadLane.Transition(nextRoadLane.transPoints, aiCar);
		}
		SpawnAICar(5f, true);
	}

	public void AddNextRoadLane(RoadLane nextRoadLane, List<Vector3> transPoints) {
		nextRoadLanes.Add(new NextRoadLane {
			roadLane = nextRoadLane,
			transPoints = transPoints
		});
	}

	public override void Clear() {
		foreach (AICar aiCar in aiCars) {
			ObjectPoolManager.Release(aiCar);
		}
		aiCars.Clear();
		base.Clear();
	}

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