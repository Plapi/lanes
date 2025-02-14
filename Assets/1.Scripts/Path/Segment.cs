using System.Collections.Generic;
using UnityEngine;

public class Segment : MonoBehaviour {

	[SerializeField] private List<LaneBase> lanes = new();
	[SerializeField] private BoxCollider boxCollider;
	
	public SegmentData SegmentData { get; private set; }

	public List<RoadLane> RoadLanes { get; private set; }
	public List<RoadLane> ForwardRoadLanes { get; private set; }
	public List<RoadLane> BackRoadLanes { get; private set; }
	
	public float Width { get; private set; }
	
	public float Length => RoadLanes[0].Length;
	
	public void Init(SegmentData segmentData) {
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
		SetBoxCollider();
	}

	public void SetStartAndEndPosForRoadLanes() {
		for (int i = 0; i < RoadLanes.Count; i++) {
			RoadLanes[i].SetStartPosAndEndPos();
		}
	}

	public void SpawnAICars(Transform parent) {
		for (int i = 0; i < RoadLanes.Count; i++) {
			RoadLanes[i].SpawnAICars(parent);
		}
		// for (int i = 0; i < ForwardRoadLanes.Count; i++) {
		// 	ForwardRoadLanes[i].SpawnAICars(parent);
		// }
		//ForwardRoadLanes[0].SpawnAICars(parent);
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

	public void Clear() {
		foreach (var lane in lanes) {
			lane.Clear();
			Destroy(lane.gameObject);
		}
		lanes.Clear();
	}

	private void SetBoxCollider() {
		if (boxCollider == null) {
			boxCollider = new GameObject("BoxCollider").AddComponent<BoxCollider>();
			boxCollider.transform.parent = transform;
			boxCollider.transform.localPosition = Vector3.zero;
		}
		boxCollider.gameObject.layer = LayerMask.NameToLayer("drivable");
		boxCollider.size = new Vector3(Width, 1f, Length);
		boxCollider.center = new Vector3(boxCollider.size.x / 2f, -0.5f, Length / 2f);
	}
}

public class SegmentData {
	public LaneData[] lanes;
}


