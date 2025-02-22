using System.Collections.Generic;
using UnityEngine;

public class Segment : MonoBehaviour {

	[SerializeField] protected List<LaneBase> lanes = new();
	
	public SegmentData SegmentData { get; protected set; }

	public List<RoadLane> RoadLanes { get; protected set; }
	public List<RoadLane> ForwardRoadLanes { get; protected set; }
	public List<RoadLane> BackRoadLanes { get; protected set; }
	
	public int Width { get; protected set; }
	
	public int Length => RoadLanes[0].Length;
	
	private SegmentEnvironment leftEnvironment;
	private SegmentEnvironment rightEnvironment;
	
	public virtual void Init(SegmentData segmentData) {
		SegmentData = segmentData;
		RoadLanes = new List<RoadLane>();
		ForwardRoadLanes = new List<RoadLane>();
		BackRoadLanes = new List<RoadLane>();
		Width = segmentData.lanes.Length * Settings.Instance.laneSize;
		for (int i = 0; i < segmentData.lanes.Length; i++) {
			LaneBase lane = Instantiate(Resources.Load<LaneBase>("Lanes/" + segmentData.lanes[i].type), transform);
			lane.name = lane.name.Replace("(Clone)", "");
			lane.transform.SetLocalX(i * Settings.Instance.laneSize);
			lane.Init(segmentData.lanes[i]);
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


