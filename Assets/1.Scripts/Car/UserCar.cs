using UnityEngine;

public class UserCar : Car {
	
	[Space]
	[SerializeField] private float accelerateSpeed;
	[SerializeField] private float breakSpeed;
	
	public int RoadLaneIndex { get; private set; }
	public Segment CurrentSegment { get; private set; }
	
	public RoadLane CurrentRoadLane => CurrentSegment.RoadLanes[RoadLaneIndex];
	
	public void SetSegment(Segment segment, int laneIndex) {
		if (CurrentSegment != null) {
			int diff = segment.BackRoadLanes.Count - CurrentSegment.BackRoadLanes.Count;
			laneIndex += diff;
			laneIndex = Mathf.Clamp(laneIndex, 0, segment.RoadLanes.Count - 1);
		} else {
			transform.SetX(segment.RoadLanes[laneIndex].transform.position.x + Settings.Instance.laneSize / 2f);
		}
		RoadLaneIndex = laneIndex;
		CurrentSegment = segment;
	}
	
	public void UpdateCar(float verticalInput) {
		targetPos = new Vector3(CurrentRoadLane.transform.position.x + Settings.Instance.laneSize / 2f, FrontPos.y, transform.position.z + 8f);
		
		float targetSpeed = verticalInput * avc.MaxSpeed;
		float accelerationInput = (targetSpeed - avc.CurrentSpeed) / avc.MaxSpeed;	
		float brakeInput = (avc.CurrentSpeed - targetSpeed) / avc.MaxSpeed;
		
		avc.ProvideInputs(GetSteering(), Mathf.Clamp(accelerationInput, 0f, 1f) * 4f, Mathf.Clamp(brakeInput, 0f, 1f));
	}
	
	public void TrySwitchLane(int add) {
		int newLaneIndex = RoadLaneIndex + add;
		if (newLaneIndex >= 0 && newLaneIndex < CurrentSegment.RoadLanes.Count) {
			RoadLaneIndex = newLaneIndex;
		}
	}

	public float GetCurrentSegmentProgress() {
		return (transform.position.z - CurrentSegment.transform.position.z) / CurrentSegment.Length;
	}

	
#if UNITY_EDITOR
	protected override void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Gizmos.DrawCube(targetPos, Vector3.one * 0.5f);
		base.OnDrawGizmos();
	}
#endif
}
