using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PathController : MonoBehaviour {

	[SerializeField] private Transform car;
	[SerializeField] private float spawnDistanceFront;
	[SerializeField] private float spawnDistanceBack;
	
	private readonly List<Segment> segments = new();
	
	private readonly LaneType[] segmentData = {
		LaneType.SideWalkLaneLeft,
		LaneType.RoadLaneSingleLeft,
		LaneType.RoadLaneMiddle,
		LaneType.RoadLaneEdgeLeft,
		LaneType.RoadLaneEdgeRight,
		LaneType.RoadLaneMiddle,
		LaneType.RoadLaneSingleRight,
		LaneType.SideWalkLaneRight
	};
	private const int segmentLength = 500;
	private int currentLength;

	private void Awake() {
		CreateNewSegment();
	}

	private void LateUpdate() {
		if (GetCarSpawnBackPos().z > segments[0].transform.localPosition.z + segmentLength) {
			segments[0].ClearLanes();
			Destroy(segments[0].gameObject);
			segments.RemoveAt(0);
		}
		if (GetCarSpawnFrontPos().z > currentLength) {
			CreateNewSegment();
		}
	}

	private void CreateNewSegment() {
		segments.Add(GetSegment());
		currentLength += segmentLength;
	}

	private Segment GetSegment() {
		Segment segment = new GameObject("Segment").AddComponent<Segment>();
		segment.transform.parent = transform;
		segment.transform.SetLocalZ(currentLength);
		segment.SetLanes(segmentData, segmentLength);
		return segment;
	}

	private Vector3 GetCarSpawnFrontPos() {
		return car.localPosition + Vector3.forward * spawnDistanceFront;
	}
	
	private Vector3 GetCarSpawnBackPos() {
		return car.localPosition + Vector3.back * spawnDistanceBack;
	}

#if UNITY_EDITOR
	private void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Gizmos.DrawCube(GetCarSpawnFrontPos(), Vector3.one * 0.5f);
		Gizmos.color = Color.red;
		Gizmos.DrawCube(GetCarSpawnBackPos(), Vector3.one * 0.5f);
	}
#endif
}
