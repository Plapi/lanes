using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PathController : MonoBehaviour {

	[SerializeField] private InputManager inputManager;
	[SerializeField] private UserCar userCar;
	[SerializeField] private Circuit circuit;
	
	private readonly List<Segment> segments = new(4);
	private Segment currentSegment;
	private Segment leftSegment;
	private Segment rightSegment;
	private Segment nextSegment;
	private Intersection intersection;
	
	private AICar[] aiCarPrefabs;
	
	private void Awake() {
		CreateCurrentSegment();
		InitSegments();
		//ShuffleArray(circuit.segments);
		// inputManager.OnHorizontalInput = input => {
		// 	userCar.TrySwitchLane((int)Mathf.Sign(input));
		// };
	}

	private void Update() {
		/*if (Input.GetKeyDown(KeyCode.C)) {
			for (int i = 0; i < segments.Count; i++) {
				segments[i].Clear();
			}
			segments.Clear();
			intersection.Clear();
			
			ShuffleArray(circuit.segments);
			InitSegments();
		}*/
		if (Input.GetKeyDown(KeyCode.N)) {
			NextSegments();
		}
		//userCar.UpdateCar(inputManager.VerticalInput);
	}

	private void InitSegments() {
		CreateNextSegments();
		for (int i = 0; i < segments.Count; i++) {
			segments[i].SetStartAndEndPosForRoadLanes();
		}
		intersection.CreateRoadConnections();
		for (int i = 0; i < segments.Count; i++) {
			segments[i].SpawnAICars();
		}
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
		leftSegment.SpawnAICars();
		rightSegment.SpawnAICars();
		nextSegment.SpawnAICars();
	}

	private void CreateCurrentSegment() {
		currentSegment = NewSegment("CurrentSegment");
		segments.Add(currentSegment);
	}
	
	private void CreateNextSegments() {
		leftSegment = NewSegment("LeftSegment", -90f);
		rightSegment = NewSegment("RightSegment", -90f);
		nextSegment = NewSegment("NextSegment");
		
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

	private Segment NewSegment(string segmentName, float angle = 0f) {
		Segment segment = new GameObject(segmentName).AddComponent<Segment>();
		segment.transform.parent = transform;
		segment.Init(GetNextSegmentData());
		segment.transform.SetLocalAngleY(angle);
		return segment;
	}

	private SegmentData GetNextSegmentData() {
		List<LaneData> lanes = new() {
			new LaneData {
				type = LaneType.SideWalkLaneLeft
			}
		};

		SegmentInputData segmentInputData = circuit.GetNextSegment();
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
			lanes[i].length = 100; //5 * Random.Range(20, 60);
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
