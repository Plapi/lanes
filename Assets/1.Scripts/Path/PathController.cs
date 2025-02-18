using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PathController : MonoBehaviourSingleton<PathController> {

	[SerializeField] private InputManager inputManager;
	[SerializeField] private UserCar userCar;
	[SerializeField] private Circuit circuit;
	
	private readonly List<Segment> segments = new(4);
	private Segment startSegment;
	private Segment currentSegment;
	private Segment leftSegment;
	private Segment rightSegment;
	private Segment nextSegment;
	private Intersection intersection;
	
	private AICar[] aiCarPrefabs;
	
	public UserCar UserCar => userCar;
	
	protected override void Awake() {
		base.Awake();
		
		ShuffleArray(circuit.segments);
		SegmentInputData inputData = circuit.GetNextSegment();
		CreateCurrentSegment(inputData);
		CreateStartSegment(inputData);
		InitSegments();
		ConnectCurrentSegmentWithStartSegment();
		
		userCar.transform.SetZ(startSegment.transform.position.z + startSegment.Length * 0.5f);
		userCar.SetSegment(startSegment, startSegment.RoadLanes.Count - 1);
		userCar.gameObject.SetActive(true);
		
		inputManager.OnHorizontalInput = input => {
			userCar.TrySwitchLane((int)Mathf.Sign(input));
		};
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.N)) {
			NextSegments();
		}
		UpdateUserCar();
	}

	private void UpdateUserCar() {
		float currentSegmentProgress = userCar.GetCurrentSegmentProgress();
		if (userCar.CurrentSegment == nextSegment) {
			if (currentSegmentProgress >= 0.5f) {
				NextSegments();
			}
		} else if (currentSegmentProgress > 0.99f) {
			userCar.SetSegment(userCar.CurrentSegment == startSegment ? currentSegment : nextSegment, userCar.RoadLaneIndex);
		}
		userCar.UpdateCar(inputManager.VerticalInput);
	}

	private void InitSegments() {
		CreateNextSegments();
		for (int i = 0; i < segments.Count; i++) {
			segments[i].SetStartAndEndPosForRoadLanes();
		}
		intersection.CreateRoadConnections();
		currentSegment.SpawnAICars();
		SpawnAICars();
	}

	private void NextSegments() {
		segments.Clear();
		currentSegment.Clear();
		leftSegment.Clear();
		rightSegment.Clear();
		intersection.Clear();
		currentSegment = nextSegment;
		currentSegment.name = "CurrentSegment";
		currentSegment.ClearNextRoadLanes();
		segments.Add(currentSegment);
		
		CreateNextSegments();
		for (int i = 0; i < segments.Count; i++) {
			segments[i].SetStartAndEndPosForRoadLanes();
		}
		intersection.CreateRoadConnections();
		SpawnAICars();
	}

	private void SpawnAICars() {
		leftSegment.SpawnAICars(false, true);
		rightSegment.SpawnAICars(true, false);
		nextSegment.SpawnAICars();
	}

	private void CreateCurrentSegment(SegmentInputData inputData) {
		currentSegment = NewSegment("CurrentSegment", inputData);
		segments.Add(currentSegment);
	}
	
	private void CreateNextSegments() {
		leftSegment = NewSegment("LeftSegment", circuit.GetNextSegment(),-90f);
		rightSegment = NewSegment("RightSegment", circuit.GetNextSegment(), -90f);
		nextSegment = NewSegment("NextSegment", circuit.GetNextSegment());
		
		segments.Add(leftSegment);
		segments.Add(rightSegment);
		segments.Add(nextSegment);
		
		leftSegment.AlignHorizontalWith(rightSegment);
		nextSegment.AlignVerticalWith(currentSegment);
		
		leftSegment.transform.SetLocalX(Mathf.Min(currentSegment.transform.localPosition.x, nextSegment.transform.localPosition.x));
		rightSegment.transform.SetLocalX(Mathf.Max(currentSegment.transform.localPosition.x + currentSegment.Width, nextSegment.transform.localPosition.x + nextSegment.Width) + rightSegment.Length);
		
		float addZ = currentSegment.transform.localPosition.z + currentSegment.Length - Mathf.Min(leftSegment.transform.localPosition.z, rightSegment.transform.localPosition.z);
		leftSegment.transform.SetLocalZ(leftSegment.transform.localPosition.z + addZ);
		rightSegment.transform.SetLocalZ(rightSegment.transform.localPosition.z + addZ);
		
		nextSegment.transform.SetLocalZ(Mathf.Max(leftSegment.transform.localPosition.z + leftSegment.Width, rightSegment.transform.localPosition.z + rightSegment.Width));
		
		intersection = Instantiate(Resources.Load<Intersection>("Intersection/Intersection"), transform);
		intersection.name = "Intersection";
		intersection.transform.SetLocalX(leftSegment.transform.localPosition.x);
		intersection.transform.SetLocalZ(currentSegment.transform.localPosition.z + currentSegment.Length);
		intersection.Init(currentSegment, leftSegment, rightSegment, nextSegment);
	}

	private void CreateStartSegment(SegmentInputData segmentInputData) {
		startSegment = new GameObject("StartSegment").AddComponent<Segment>();
		startSegment.transform.parent = transform;
		const int length = 200;
		SegmentData segmentData = GetNextSegmentData(segmentInputData);
		for (int i = 0; i < segmentData.lanes.Length; i++) {
			segmentData.lanes[i].length = length;
		}
		startSegment.Init(segmentData);
		startSegment.transform.SetLocalZ(-length);
		startSegment.SetStartAndEndPosForRoadLanes();
	}

	private Segment NewSegment(string segmentName, SegmentInputData inputData, float angle = 0f) {
		Segment segment = new GameObject(segmentName).AddComponent<Segment>();
		segment.transform.parent = transform;
		segment.Init(GetNextSegmentData(inputData));
		segment.transform.SetLocalAngleY(angle);
		return segment;
	}

	private void ConnectCurrentSegmentWithStartSegment() {
		for (int i = 0; i < currentSegment.BackRoadLanes.Count; i++) {
			RoadLane lane0 = currentSegment.BackRoadLanes[i];
			RoadLane lane1 = startSegment.BackRoadLanes[i];
			lane0.AddNextRoadLane(lane1, new List<Vector3> { lane0.EndPos, lane1.StartPos });
		}
	}

	private static SegmentData GetNextSegmentData(SegmentInputData segmentInputData) {
		List<LaneData> lanes = new() {
			new LaneData {
				type = LaneType.SideWalkLaneLeft
			}
		};
		
		segmentInputData.length = 5 * Random.Range(30, 80);
		int backLanes = segmentInputData.backLanes;
		int frontLanes = segmentInputData.frontLanes;

		if (backLanes > 1) {
			lanes.Add(new RoadLaneData {
				type = LaneType.RoadLaneSingleLeft
			});	
			for (int i = 1; i < backLanes - 1; i++) {
				lanes.Add(new RoadLaneData {
					type = LaneType.RoadLaneMiddle
				});	
			}
		}
		lanes.Add(new RoadLaneData {
			type = LaneType.RoadLaneEdgeLeft
		});
		
		lanes.Add(new RoadLaneData {
			type = LaneType.RoadLaneEdgeRight,
			hasFrontDirection = true,
		});
		if (frontLanes > 1) {
			for (int i = 1; i < frontLanes - 1; i++) {
				lanes.Add(new RoadLaneData {
					type = LaneType.RoadLaneMiddle,
					hasFrontDirection = true
				});	
			}
			lanes.Add(new RoadLaneData {
				type = LaneType.RoadLaneSingleRight,
				hasFrontDirection = true,
			});
		}
		
		lanes.Add(new LaneData {
			type = LaneType.SideWalkLaneRight
		});
		for (int i = 0; i < lanes.Count; i++) {
			lanes[i].length = segmentInputData.length;
		}
		
		return new SegmentData {
			lanes = lanes.ToArray()
		};
	}
	
	[Serializable]
	private class Circuit {
		public SegmentInputData[] segments;
		private int currentSegmentIndex;

		public SegmentInputData GetNextSegment() {
			if (currentSegmentIndex >= segments.Length) {
				currentSegmentIndex = 0;
			}
			return segments[currentSegmentIndex++];
		}
	}
	
	[Serializable]
	private class SegmentInputData {
		[Range(1, 4)] public int backLanes = 2;
		[Range(1, 4)] public int frontLanes = 2;
		public int length;
	}
	
	private static void ShuffleArray<T>(T[] array) {
		System.Random rng = new System.Random();
		int n = array.Length;
		for (int i = n - 1; i > 0; i--) {
			int j = rng.Next(i + 1);
			(array[i], array[j]) = (array[j], array[i]);
		}
	}
}
