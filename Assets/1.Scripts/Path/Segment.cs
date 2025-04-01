using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Segment : MonoBehaviour {

	[SerializeField] protected List<LaneBase> lanes = new();
	
	public SegmentData SegmentData { get; protected set; }

	public List<LaneBase> Lanes => lanes;
	public List<RoadLane> RoadLanes { get; protected set; }
	public List<RoadLane> ForwardRoadLanes { get; protected set; }
	public List<RoadLane> BackRoadLanes { get; protected set; }
	
	public int Width { get; protected set; }
	
	public int Length => RoadLanes[0].Length;
	public int LeftLength { get; private set; }
	public int RightLength { get; private set; }

	public SideEnvironment LeftEnvironment { get; set; }
	public SideEnvironment RightEnvironment { get; set; }

	private readonly GameObject[] laneMeshObjects = new GameObject[4];
	
	public virtual void Init(SegmentData segmentData) {
		SegmentData = segmentData;
		RoadLanes = new List<RoadLane>();
		ForwardRoadLanes = new List<RoadLane>();
		BackRoadLanes = new List<RoadLane>();
		Width = segmentData.lanes.Length * Settings.Instance.laneSize;
		for (int i = 0; i < segmentData.lanes.Length; i++) {
			
			LaneType laneType = segmentData.lanes[i].type;
			Type type = laneType == LaneType.SideWalk ? typeof(SideWalkLane) : typeof(RoadLane);
			LaneBase lane = (LaneBase)new GameObject(segmentData.lanes[i].type.ToString()).AddComponent(type);
			
			lane.transform.parent = transform;
			lane.transform.SetLocalX(i * Settings.Instance.laneSize);
			lane.Init(segmentData.lanes[i]);
			lane.meshObj = GetLaneMeshObj(segmentData.lanes[i], lane.transform);
			
			lanes.Add(lane);
			if (lane is RoadLane roadLane) {
				RoadLanes.Add(roadLane);
				if (roadLane.Data.hasFrontDirection) {
					ForwardRoadLanes.Insert(0, roadLane);
				} else {
					BackRoadLanes.Add(roadLane);
				}
			}
		}
	}

	private GameObject GetLaneMeshObj(LaneData laneData, Transform parent) {
		int laneIndex = (int)laneData.type;
		if (laneMeshObjects[laneIndex] == null) {
			laneMeshObjects[laneIndex] = GeneratorsController.Instance.LaneGenerator.Generate(parent, laneData);
			return laneMeshObjects[laneIndex];
		}
		GameObject obj = Instantiate(laneMeshObjects[laneIndex], parent);
		obj.transform.localPosition = new Vector3(Settings.Instance.laneSize, 0, laneData.length);
		obj.transform.SetAngleY(180f);
		return obj;
	}

	public void SetStartAndEndPosForRoadLanes() {
		for (int i = 0; i < RoadLanes.Count; i++) {
			RoadLanes[i].SetStartPosAndEndPos();
		}
	}

	public void SpawnAICars(bool forward = true, bool back = true) {
		if (forward && back) {
			for (int i = 0; i < RoadLanes.Count; i++) {
				RoadLanes[i].SpawnAICars();
			}	
		} else if (forward) {
			for (int i = 0; i < ForwardRoadLanes.Count; i++) {
				ForwardRoadLanes[i].SpawnAICars();
			}
		} else {
			for (int i = 0; i < BackRoadLanes.Count; i++) {
				BackRoadLanes[i].SpawnAICars();
			}
		}
	}

	public void SetLengthSides(int leftSideWalks, int rightSideWalks) {
		LeftLength = leftSideWalks * Settings.Instance.laneSize + Length;
		RightLength = rightSideWalks * Settings.Instance.laneSize + Length;
	}

	public void ClearNextRoadLanes() {
		for (int i = 0; i < RoadLanes.Count; i++) {
			RoadLanes[i].ClearNextRoadLanes();
		}
	}

	public void CreateInitEnv() {
		LeftEnvironment = CreateEnv("LeftEnv");
		LeftEnvironment.transform.position = transform.position + transform.forward * Length;
		LeftEnvironment.Generate(LeftEnvironment.transform.position - transform.forward * Length - transform.right * 10f, true);
		RightEnvironment = CreateEnv("RightEnv");
		RightEnvironment.transform.position = LeftEnvironment.transform.position + transform.right * Width;
		RightEnvironment.Generate(RightEnvironment.transform.position - transform.forward * Length + transform.right * 10f, false);
	}
	
	public void CreateBottomEnv() {
		LeftEnvironment = CreateEnv("LeftEnv");
		LeftEnvironment.Generate(LeftEnvironment.transform.position + transform.forward * LeftLength - transform.right * 10f, true);
		RightEnvironment = CreateEnv("RightEnv");
		RightEnvironment.transform.position = LeftEnvironment.transform.position + transform.right * Width;
		RightEnvironment.Generate(RightEnvironment.transform.position + transform.forward * RightLength + transform.right * 10f, false);
	}
	
	public void CreateTopEnv() {
		LeftEnvironment = CreateEnv("LeftEnv");
		LeftEnvironment.transform.position = transform.position + transform.forward * Length;
		LeftEnvironment.Generate(LeftEnvironment.transform.position - transform.forward * RightLength - transform.right * 10f, true);
		RightEnvironment = CreateEnv("RightEnv");
		RightEnvironment.transform.position = LeftEnvironment.transform.position + transform.right * Width;
		RightEnvironment.Generate(RightEnvironment.transform.position - transform.forward * LeftLength + transform.right * 10f, false);
	}

	public void CreateSideEnv() {
		LeftEnvironment = CreateEnv("LeftEnv");
		LeftEnvironment.transform.position += transform.forward * Length;
		LeftEnvironment.Generate(LeftEnvironment.transform.position - transform.forward * (RightLength - 10f) - transform.right * 10f, true);
		RightEnvironment = CreateEnv("RightEnv");
		RightEnvironment.transform.position = LeftEnvironment.transform.position + transform.right * Width;
		RightEnvironment.Generate(RightEnvironment.transform.position - transform.forward * (LeftLength - 10f) + transform.right * 10f, false);
	}

	protected SideEnvironment CreateEnv(string name) {
		SideEnvironment env = new GameObject(name).AddComponent<SideEnvironment>();
		env.transform.parent = transform;
		env.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		return env;
	}

	public static Segment Create(Transform parent, string segmentName, float angle = 0f) {
		return Create(parent, segmentName, GetRandomSegmentData(), angle);
	}
	
	public static Segment Create(Transform parent, string segmentName, int length, float angle = 0f) {
		return Create(parent, segmentName, GetRandomSegmentData(length), angle);
	}

	public static Segment Create(Transform parent, string segmentName, SegmentInputData segmentInputData, float angle = 0f) {
		return Create(parent, segmentName, GetSegmentData(segmentInputData), angle);
	}

	public static Segment Create(Transform parent, string segmentName, SegmentData segmentData, float angle = 0f) {
		Segment segment = new GameObject(segmentName).AddComponent<Segment>();
		segment.transform.parent = parent;
		segment.Init(segmentData);
		segment.transform.SetLocalAngleY(angle);
		return segment;
	}
	
	public static SegmentData GetSegmentData(SegmentInputData segmentInputData) {
		List<LaneData> lanes = new() {
			new LaneData {
				type = LaneType.SideWalk
			}
		};
		
		int backLanes = segmentInputData.backLanes;
		int frontLanes = segmentInputData.frontLanes;

		if (backLanes > 1) {
			lanes.Add(new RoadLaneData {
				type = LaneType.RoadFirst
			});	
			for (int i = 1; i < backLanes - 1; i++) {
				lanes.Add(new RoadLaneData {
					type = LaneType.RoadMiddle
				});	
			}
		}
		lanes.Add(new RoadLaneData {
			type = LaneType.RoadLast
		});
		
		lanes.Add(new RoadLaneData {
			type = LaneType.RoadLast,
			hasFrontDirection = true,
		});
		if (frontLanes > 1) {
			for (int i = 1; i < frontLanes - 1; i++) {
				lanes.Add(new RoadLaneData {
					type = LaneType.RoadMiddle,
					hasFrontDirection = true
				});	
			}
			lanes.Add(new RoadLaneData {
				type = LaneType.RoadFirst,
				hasFrontDirection = true,
			});
		}
		
		lanes.Add(new LaneData {
			type = LaneType.SideWalk
		});
		for (int i = 0; i < lanes.Count; i++) {
			lanes[i].length = segmentInputData.length;
		}
		
		return new SegmentData {
			lanes = lanes.ToArray()
		};
	}
	
	private static SegmentData GetRandomSegmentData(int length = -1) {
		SegmentInputData segmentInputData = new() {
			backLanes = Random.Range(1, 5),
			frontLanes = Random.Range(1, 5),
			length = length == -1 ? Settings.Instance.laneSize * Random.Range(30, 50) : length
		};
		return GetSegmentData(segmentInputData);
	}

	public void ClearAICars() {
		foreach (var roadLane in RoadLanes) {
			roadLane.ClearAICars();
		}
	}
	
	public void Clear() {
		foreach (var lane in lanes) {
			lane.Clear();
			Destroy(lane.gameObject);
		}
		lanes.Clear();
		if (LeftEnvironment != null) {
			LeftEnvironment.Clear();
		}
		if (RightEnvironment != null) {
			RightEnvironment.Clear();
		}
		Destroy(gameObject);
	}
}

public class SegmentData {
	public LaneData[] lanes;
}

public class SegmentInputData {
	public int backLanes = 2;
	public int frontLanes = 2;
	public int length = 100;
}


