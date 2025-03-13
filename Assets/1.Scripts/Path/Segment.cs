using System;
using System.Collections.Generic;
using UnityEngine;

public class Segment : MonoBehaviour {

	[SerializeField] protected List<LaneBase> lanes = new();
	
	public SegmentData SegmentData { get; protected set; }

	public List<LaneBase> Lanes => lanes;
	public List<RoadLane> RoadLanes { get; protected set; }
	public List<RoadLane> ForwardRoadLanes { get; protected set; }
	public List<RoadLane> BackRoadLanes { get; protected set; }
	
	public int Width { get; protected set; }
	
	public int Length => RoadLanes[0].Length;
	
	private SegmentEnvironment leftEnvironment;
	private SegmentEnvironment rightEnvironment;
	
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
			laneMeshObjects[laneIndex] = LaneGenerator.Instance.Generate(parent, laneData);
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

	public void ClearNextRoadLanes() {
		for (int i = 0; i < RoadLanes.Count; i++) {
			RoadLanes[i].ClearNextRoadLanes();
		}
	}

	public void AlignVerticalWith(Segment other) {
		int backLanesCount0 = BackRoadLanes.Count;
		int backLanesCount1 = other.BackRoadLanes.Count;
		transform.SetLocalX(other.transform.localPosition.x + (backLanesCount1 - backLanesCount0) * Settings.Instance.laneSize);
	}

	public void AlignHorizontalWith(Segment other) {
		int backLanesCount0 = BackRoadLanes.Count;
		int backLanesCount1 = other.BackRoadLanes.Count;
		transform.SetLocalZ(other.transform.localPosition.z + (backLanesCount1 - backLanesCount0) * Settings.Instance.laneSize);
	}

	public void CreateBottomLeftEnvironment(Segment leftSegment) {
		SegmentEnvironment segmentEnvironment = CreateEnvironment();
		segmentEnvironment.Create((int)transform.InverseTransformPoint(leftSegment.transform.position).z, true);
		leftEnvironment = segmentEnvironment;
	}

	public void CreateBottomRightEnvironment(Segment rightSegment) {
		SegmentEnvironment segmentEnvironment = CreateEnvironment();
		segmentEnvironment.transform.SetLocalX(Width);
		segmentEnvironment.Create((int)transform.InverseTransformPoint(rightSegment.transform.position).z, false);
		rightEnvironment = segmentEnvironment;
	}

	public void CreteTopLeftEnvironment(Segment leftSegment) {
		SegmentEnvironment segmentEnvironment = CreateEnvironment();
		segmentEnvironment.transform.SetZ(leftSegment.transform.position.z + leftSegment.Width + Settings.Instance.laneSize);
		segmentEnvironment.Create((int)((transform.position.z + Length) - (leftSegment.transform.position.z + leftSegment.Width)) - Settings.Instance.laneSize, true);
		leftEnvironment = segmentEnvironment;
	}

	public void CreteTopRightEnvironment(Segment rightSegment) {
		SegmentEnvironment segmentEnvironment = CreateEnvironment();
		segmentEnvironment.transform.SetLocalX(Width);
		segmentEnvironment.transform.SetZ(rightSegment.transform.position.z + rightSegment.Width + Settings.Instance.laneSize);
		segmentEnvironment.Create((int)((transform.position.z + Length) - (rightSegment.transform.position.z + rightSegment.Width)) - Settings.Instance.laneSize, false);
		rightEnvironment = segmentEnvironment;
	}

	public void ContinueGenerateEnvIfNeeded(Segment leftSegment, Segment rightSegment) {
		leftEnvironment.ContinueGenerateIfNeeded((int)(transform.InverseTransformPoint(leftSegment.transform.position).z - leftEnvironment.transform.localPosition.z));
		rightEnvironment.ContinueGenerateIfNeeded((int)(transform.InverseTransformPoint(rightSegment.transform.position).z - rightEnvironment.transform.localPosition.z));
	}
	
	protected SegmentEnvironment CreateEnvironment() {
		SegmentEnvironment segmentEnvironment = new GameObject().AddComponent<SegmentEnvironment>();
		segmentEnvironment.name = "SegmentEnvironment";
		segmentEnvironment.transform.parent = transform;
		segmentEnvironment.transform.localPosition = Vector3.zero;
		return segmentEnvironment;
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
		if (leftEnvironment != null) {
			leftEnvironment.Clear();
		}
		if (rightEnvironment != null) {
			rightEnvironment.Clear();
		}
		Destroy(gameObject);
	}
}

public class SegmentData {
	public LaneData[] lanes;
}


