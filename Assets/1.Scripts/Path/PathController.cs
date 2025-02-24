using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PathController : MonoBehaviourSingleton<PathController> {

	[SerializeField] private InputManager inputManager;
	[SerializeField] private UserCar userCar;
	[SerializeField] private Transform skyline;
	[SerializeField] private StartSegment startSegment;
	
	[Space]
	[SerializeField] private bool userCarEnabled;
	[SerializeField] private bool aiCarsEnabled;
	
	private readonly List<Segment> segments = new(4);
	private Segment currentSegment;
	private Segment leftSegment;
	private Segment rightSegment;
	private Segment nextSegment;
	private Intersection intersection;
	
	private AICar[] aiCarPrefabs;
	
	public UserCar UserCar => userCar;

	protected override void Awake() {
		base.Awake();
		InitFirstSegments();
		InitUserCar();
	}

	private void Update() {
		if (userCarEnabled) {
			userCar.UpdateCar(inputManager.VerticalInput, inputManager.HorizontalInput);
			skyline.transform.position = userCar.transform.position;
		} else {
			skyline.gameObject.SetActive(false);
		}
	}

	private void InitUserCar() {
		if (!userCarEnabled) {
			return;
		}
		userCarEnabled = false;
		userCar.SetSegments(startSegment, currentSegment);
		userCar.SetStartPoints();
		userCar.GoToStart(() => {
			userCarEnabled = true;
		});
		userCar.OnRequireNewSegments = () => {
			if (userCar.CurrentSegment != startSegment) {
				startSegment.ClearAICars();
				InitNextSegments();
			}
			userCar.SetSegments(currentSegment, nextSegment);
		};
	}

	private void InitFirstSegments() {
		startSegment.Init(GetSegmentData(new SegmentInputData {
			backLanes = 2,
			frontLanes = 2,
			length = 200
		}));
		startSegment.SetStartAndEndPosForRoadLanes();
		currentSegment = NewSegment("CurrentSegment", GetSegmentData(new SegmentInputData {
			backLanes = 2,
			frontLanes = 2,
			length = 50
		}));
		segments.Add(currentSegment);
		CreateNextSegments();
		for (int i = 0; i < segments.Count; i++) {
			segments[i].SetStartAndEndPosForRoadLanes();
		}
		intersection.CreateRoadConnections();
		if (aiCarsEnabled) {
			currentSegment.SpawnAICars();
			SpawnAICars();	
		}
		ConnectCurrentSegmentWithStartSegment();
		startSegment.CreateBottomLeftEnvironment(leftSegment);
		startSegment.CreateRightEnvironment(rightSegment);
		nextSegment.CreteTopLeftEnvironment(leftSegment);
		nextSegment.CreteTopRightEnvironment(rightSegment);
	}

	private void InitNextSegments() {
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
		
		currentSegment.ContinueGenerateEnvIfNeeded(leftSegment, rightSegment);
		nextSegment.CreteTopLeftEnvironment(leftSegment);
		nextSegment.CreteTopRightEnvironment(rightSegment);

		if (aiCarsEnabled) {
			SpawnAICars();
		}
	}

	private void SpawnAICars() {
		leftSegment.SpawnAICars(false, true);
		rightSegment.SpawnAICars(true, false);
		nextSegment.SpawnAICars();
	}
	
	private void CreateNextSegments() {
		leftSegment = NewSegment("LeftSegment", GetRandomSegmentData(),-90f);
		rightSegment = NewSegment("RightSegment", GetRandomSegmentData(), -90f);
		nextSegment = NewSegment("NextSegment", GetRandomSegmentData());
		
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

	private Segment NewSegment(string segmentName, SegmentData segmentData, float angle = 0f) {
		Segment segment = new GameObject(segmentName).AddComponent<Segment>();
		segment.transform.parent = transform;
		segment.Init(segmentData);
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

	private static SegmentData GetRandomSegmentData() {
		SegmentInputData segmentInputData = new() {
			backLanes = Random.Range(1, 4),
			frontLanes = Random.Range(1, 4),
			length = Settings.Instance.laneSize * Random.Range(30, 80)
		};
		return GetSegmentData(segmentInputData);
	}

	private static SegmentData GetSegmentData(SegmentInputData segmentInputData) {
		List<LaneData> lanes = new() {
			new LaneData {
				type = LaneType.SideWalkLaneLeft
			}
		};
		
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
	private class SegmentInputData {
		[Range(1, 4)] public int backLanes = 2;
		[Range(1, 4)] public int frontLanes = 2;
		public int length;
	}
}
