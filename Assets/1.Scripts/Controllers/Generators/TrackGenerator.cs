using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrackGenerator : MonoBehaviourSingleton<TrackGenerator> {

	[SerializeField] private Segment startSegment;
	
	[Space]
	[SerializeField] private bool aiCarsEnabled;
	[SerializeField] private float spawnAICarDistanceStartMin;
	[SerializeField] private float spawnAICarDistanceStartMax;
	[SerializeField] private float spawnAICarDistanceEndMin; // 20
	[SerializeField] private float spawnAICarDistanceEndMax; // 40
	[SerializeField] private float spawnAICarDistanceDecrease;
	
	private Segment currentSegment;
	private Segment bottomSegment;
	private Segment leftSegment;
	private Segment rightSegment;
	private Segment topSegment;
	private Segment[] segments;
	private Intersection intersection;

	private float spawnAICarDistanceMin;
	private float spawnAICarDistanceMax;

	public Action<bool> OnStartSegmentSetActive;
	
	public void Init(GenerateDir dir) {
		spawnAICarDistanceMin = spawnAICarDistanceStartMin;
		spawnAICarDistanceMax = spawnAICarDistanceStartMax;
		if (startSegment == null) {
			currentSegment = Segment.Create(transform, "CurrentSegment", new SegmentInputData { length = 200 });
			currentSegment.CreateInitEnv();	
		} else {
			currentSegment = startSegment;
			currentSegment.Init(Segment.GetSegmentData(new SegmentInputData { length = 200 }));
		}
		CreateNextSegments(dir);
	}

	public void SetSpawnAICarDistance(float startMin, float startMax, float endMin, float endMax) {
		spawnAICarDistanceMin = startMin;
		spawnAICarDistanceMax = startMax;
		spawnAICarDistanceEndMin = endMin;
		spawnAICarDistanceEndMax = endMax;
	}

	public Segment GetCurrentSegment() {
		return bottomSegment;
	}

	public Segment GetNextSegment(GenerateDir dir) {
		return dir switch {
			GenerateDir.Forward => topSegment,
			GenerateDir.Left => leftSegment,
			GenerateDir.Right => rightSegment,
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	private void CreateNextSegments(GenerateDir dir) {
		int laneSize = Settings.Instance.laneSize;

		SegmentData bottomSData = new SegmentData {
			lanes = new LaneData[currentSegment.SegmentData.lanes.Length]
		};
		for (int i = 0; i < bottomSData.lanes.Length; i++) {
			if (currentSegment.SegmentData.lanes[i] is RoadLaneData rlData) {
				bottomSData.lanes[i] = new RoadLaneData {
					type = rlData.type,
					length = 100,
					hasFrontDirection = rlData.hasFrontDirection
				};
			} else {
				bottomSData.lanes[i] = new LaneData {
					type = currentSegment.SegmentData.lanes[i].type,
					length = 100
				};
			}
		}
		bottomSegment = Segment.Create(transform, "BottomSegment", bottomSData);
		bottomSegment.transform.position = currentSegment.transform.position + currentSegment.transform.forward * currentSegment.Length;
		bottomSegment.transform.SetLocalAngleY(currentSegment.transform.localEulerAngles.y);
		
		leftSegment = Segment.Create(transform, "LeftSegment", dir != GenerateDir.Left ? 100 : -1, bottomSegment.transform.localEulerAngles.y - 90f);
		rightSegment = Segment.Create(transform, "RightSegment", dir != GenerateDir.Right ? 100 : -1, bottomSegment.transform.localEulerAngles.y + 90f);
		topSegment = Segment.Create(transform, "TopSegment", dir != GenerateDir.Forward ? 100 : -1, bottomSegment.transform.localEulerAngles.y);
		
		segments = new[] { currentSegment, bottomSegment, leftSegment, rightSegment, topSegment };
		
		float verDiff = (bottomSegment.BackRoadLanes.Count - topSegment.BackRoadLanes.Count) * laneSize;
		
		leftSegment.transform.position = bottomSegment.transform.position + bottomSegment.transform.forward * bottomSegment.Length;
		rightSegment.transform.position = bottomSegment.transform.position + bottomSegment.transform.forward * (bottomSegment.Length + rightSegment.Width);
		topSegment.transform.position = bottomSegment.transform.position + bottomSegment.transform.forward * bottomSegment.Length + 
			bottomSegment.transform.right * verDiff;
		
		if (topSegment.BackRoadLanes.Count > bottomSegment.BackRoadLanes.Count) {
			leftSegment.transform.position += topSegment.transform.right * verDiff;
		}
		if (leftSegment.BackRoadLanes.Count != rightSegment.ForwardRoadLanes.Count) {
			Segment minSegment = leftSegment.BackRoadLanes.Count < rightSegment.ForwardRoadLanes.Count ? leftSegment : rightSegment;
			minSegment.transform.position += bottomSegment.transform.forward * 
				(Mathf.Abs(leftSegment.BackRoadLanes.Count - rightSegment.ForwardRoadLanes.Count) * laneSize);
			topSegment.transform.position += bottomSegment.transform.forward * 
				((Mathf.Max(leftSegment.BackRoadLanes.Count, rightSegment.ForwardRoadLanes.Count) +
				  Mathf.Max(leftSegment.ForwardRoadLanes.Count, rightSegment.BackRoadLanes.Count) + 2) * laneSize);
		} else {
			topSegment.transform.position += bottomSegment.transform.forward * Mathf.Max(leftSegment.Width, rightSegment.Width);
		}
		
		rightSegment.transform.position += bottomSegment.transform.right * 
			((bottomSegment.BackRoadLanes.Count + Mathf.Max(bottomSegment.ForwardRoadLanes.Count, topSegment.ForwardRoadLanes.Count) + 2) * laneSize);
		
		intersection = Instantiate(Resources.Load<Intersection>("Intersection/Intersection"), transform);
		intersection.name = "Intersection";
		Utils.GetIntersection(bottomSegment.transform.position + bottomSegment.transform.forward * (bottomSegment.Length + laneSize), 
			-bottomSegment.transform.right,
			leftSegment.transform.position - leftSegment.transform.forward * laneSize,
			leftSegment.transform.right, out Vector3 p0);
		intersection.transform.position = p0;
		intersection.Init(bottomSegment, leftSegment, rightSegment, topSegment);
		
		currentSegment.ClearNextRoadLanes();
		for (int i = 0; i < segments.Length; i++) {
			segments[i].SetStartAndEndPosForRoadLanes();
		}
		intersection.CreateRoadConnections();
		ConnectCurrentSegmentWithBottomSegment();
		
		bottomSegment.CreateBottomEnv();
		topSegment.CreateTopEnv();
		leftSegment.CreateSideEnv();
		rightSegment.CreateSideEnv();
	}

	public void SpawnAICars() {
		if (!aiCarsEnabled) {
			return;
		}
		for (int i = 1; i < segments.Length; i++) { 
			segments[i].SpawnAICars();
		}	
	}
	
	private void ConnectCurrentSegmentWithBottomSegment() {
		for (int i = 0; i < currentSegment.ForwardRoadLanes.Count; i++) {
			RoadLane lane0 = currentSegment.ForwardRoadLanes[i];
			RoadLane lane1 = bottomSegment.ForwardRoadLanes[i];
			Vector3 dir = (lane0.EndPos - lane0.StartPos).normalized * 2f;
			lane0.AddNextRoadLane(lane1, new List<Vector3> { lane0.EndPos, lane1.StartPos + dir });
		}
		for (int i = 0; i < bottomSegment.BackRoadLanes.Count; i++) {
			RoadLane lane0 = bottomSegment.BackRoadLanes[i];
			RoadLane lane1 = currentSegment.BackRoadLanes[i];
			Vector3 dir = (lane0.EndPos - lane0.StartPos).normalized * 2f;
			lane0.AddNextRoadLane(lane1, new List<Vector3> { lane0.EndPos, lane1.StartPos + dir });
		}
	}

	public void ClearAndReset(GenerateDir dir) {
		ClearAllSegments();
		startSegment.gameObject.SetActive(true);
		OnStartSegmentSetActive?.Invoke(true);
		Init(dir);
	}

	public void Generate(GenerateDir prevDir, GenerateDir nextDir) {
		Segment segment = GetNextSegment(prevDir);
		ClearAllSegmentsExcept(segment);
		currentSegment = segment;
		currentSegment.name = "CurrentSegment";
		CreateNextSegments(nextDir);
		SpawnAICars();
	}

	private void ClearAllSegmentsExcept(Segment exceptSegment) {
		for (int i = 0; i < segments.Length; i++) {
			if (segments[i] != exceptSegment && segments[i] != startSegment) {
				segments[i].Clear();
			}
		}
		intersection.Clear();
		if (startSegment != null) {
			startSegment.ClearAICars();
			startSegment.gameObject.SetActive(false);
			OnStartSegmentSetActive?.Invoke(false);
		}
	}

	public void ClearAllSegments() {
		for (int i = 0; i < segments.Length; i++) {
			if (segments[i] != startSegment) {
				segments[i].Clear();	
			}
		}
		intersection.Clear();
	}
	
	public float GetSpawnAICarDistance() {
		spawnAICarDistanceMin = Mathf.Clamp(spawnAICarDistanceMin - spawnAICarDistanceDecrease,
			spawnAICarDistanceEndMin, spawnAICarDistanceStartMin);
		spawnAICarDistanceMax = Mathf.Clamp(spawnAICarDistanceMax - spawnAICarDistanceDecrease,
			spawnAICarDistanceEndMax, spawnAICarDistanceStartMax);
		return Random.Range(spawnAICarDistanceMin, spawnAICarDistanceMax);
	}
	
	/*private void Update() {
		if (Input.GetKeyDown(KeyCode.R)) {
			ClearAndReset(GenerateDir.Forward);
		}
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			Generate(GenerateDir.Forward, GenerateDir.Forward);
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			Generate(GenerateDir.Left, GenerateDir.Left);
		}
		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			Generate(GenerateDir.Right, GenerateDir.Right);
		}
	}*/
}

public enum GenerateDir {
	Forward,
	Left,
	Right
}
