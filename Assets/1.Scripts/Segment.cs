using System.Collections.Generic;
using UnityEngine;

public class Segment : MonoBehaviour {

	[SerializeField] private List<LaneBase> lanes = new();
	[SerializeField] private BoxCollider boxCollider;
	
	public SegmentData SegmentData { get; private set; }

	public List<RoadLane> RoadLanes { get; private set; }
	public List<RoadLane> ForwardRoadLanes { get; private set;}
	public List<RoadLane> BackRoadLanes { get; private set;}
	
	public void Init(SegmentData segmentData) {
		SegmentData = segmentData;
		RoadLanes = new List<RoadLane>();
		ForwardRoadLanes = new List<RoadLane>();
		BackRoadLanes = new List<RoadLane>();
		ClearLanes();
		for (int i = 0; i < segmentData.lanes.Length; i++) {
			LaneBase lane = Instantiate(Resources.Load<LaneBase>("Lanes/" + segmentData.lanes[i].type), transform);
			lane.Init(segmentData.lanes[i]);
			lane.name = lane.name.Replace("(Clone)", "");
			lane.transform.SetLocalX(i * Settings.Instance.laneSize);
			lane.SetElements(segmentData.length);
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
		SetBoxCollider(segmentData.length);
	}

	public void AlignWith(Segment other) {
		int backLanesCount0 = BackRoadLanes.Count;
		int backLanesCount1 = other.BackRoadLanes.Count;
		transform.SetLocalX(other.transform.localPosition.x + (backLanesCount1 - backLanesCount0) * Settings.Instance.laneSize);
	}

	public void ClearLanes() {
		foreach (var lane in lanes) {
			if (Application.isPlaying) {
				lane.ClearElements();
				Destroy(lane.gameObject);	
			} else {
				DestroyImmediate(lane.gameObject);
			}
		}
		lanes.Clear();
	}

	private void SetBoxCollider(float length) {
		if (boxCollider == null) {
			boxCollider = new GameObject("BoxCollider").AddComponent<BoxCollider>();
			boxCollider.transform.parent = transform;
			boxCollider.transform.localPosition = Vector3.zero;
		}
		boxCollider.gameObject.layer = LayerMask.NameToLayer("drivable");
		boxCollider.size = new Vector3(Settings.Instance.laneSize * lanes.Count, 1f, length);
		boxCollider.center = new Vector3(boxCollider.size.x / 2f, -0.5f, length / 2f);
	}
	
#if UNITY_EDITOR
	[Space]
	[SerializeField] [Range(0, 500)] private int debugLength = 500;
	[SerializeField] private LaneType[] debugLaneTypes;
	[ContextMenu("Set Debug Lanes")]
	private void SetDebugLanes() {
		//SetLanes(debugLaneTypes, debugLength);
	}
#endif
}

public class SegmentData {
	public LaneData[] lanes;
	public int length;
}


