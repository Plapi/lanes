using System.Collections.Generic;
using UnityEngine;

public class Segment : MonoBehaviour {

	[SerializeField] private List<Lane> lanes = new();
	[SerializeField] private BoxCollider boxCollider;
	
	private void SetLanes(LaneType[] laneTypes, float length) {
		foreach (var lane in lanes) {
			if (Application.isPlaying) {
				Destroy(lane.gameObject);	
			} else {
				DestroyImmediate(lane.gameObject);
			}
		}
		lanes.Clear();
		for (int i = 0; i < laneTypes.Length; i++) {
			Lane lane = Instantiate(Resources.Load<Lane>("Lanes/" + laneTypes[i]), transform);
			lane.name = lane.name.Replace("(Clone)", "");
			lane.transform.SetLocalX(i * 5f);
			lane.SetElements(length);
			lanes.Add(lane);
		}
		
	}

	[ContextMenu("Create Box Collider")]
	private void CreateBoxCollider() {
		if (boxCollider == null) {
			boxCollider = new GameObject("boxCollider").AddComponent<BoxCollider>();
		}
	}
	
#if UNITY_EDITOR
	[SerializeField] [Range(0, 500)] private float debugLength;
	[SerializeField] private LaneType[] debugLaneTypes;
	[ContextMenu("Set Debug Lanes")]
	private void SetDebugLanes() {
		SetLanes(debugLaneTypes, debugLength);
	}
#endif
	
	private enum LaneType {
		RoadLaneSingleLeft,
		RoadLaneSingleRight,
		RoadLaneMiddle,
		RoadLaneEdgeLeft,
		RoadLaneEdgeRight,
		SideWalkLaneRight,
		SideWalkLaneLeft
	}
}

