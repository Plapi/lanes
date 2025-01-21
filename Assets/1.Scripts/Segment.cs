using System.Collections.Generic;
using UnityEngine;

public class Segment : MonoBehaviour {

	[SerializeField] private List<Lane> lanes = new();
	[SerializeField] private BoxCollider boxCollider;
	
	public void SetLanes(LaneType[] laneTypes, int length) {
		ClearLanes();
		for (int i = 0; i < laneTypes.Length; i++) {
			Lane lane = Instantiate(Resources.Load<Lane>("Lanes/" + laneTypes[i]), transform);
			lane.name = lane.name.Replace("(Clone)", "");
			lane.transform.SetLocalX(i * Settings.Instance.laneSize);
			lane.SetElements(length);
			lanes.Add(lane);
		}
		SetBoxCollider(length);
	}

	private void ClearLanes() {
		foreach (var lane in lanes) {
			if (Application.isPlaying) {
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
		SetLanes(debugLaneTypes, debugLength);
	}
#endif
}

public enum LaneType {
	RoadLaneSingleLeft,
	RoadLaneSingleRight,
	RoadLaneMiddle,
	RoadLaneEdgeLeft,
	RoadLaneEdgeRight,
	SideWalkLaneRight,
	SideWalkLaneLeft
}

