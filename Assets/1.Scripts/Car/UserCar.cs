using UnityEngine;

public class UserCar : Car {
	
	[Space]
	[SerializeField] private float accelerateSpeed;
	[SerializeField] private float breakSpeed;
	
	[Space]
	[SerializeField] private float requireNewSegmentOffset = 100;
	[SerializeField] private float releaseSegmentOffset = 10;
	
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
		targetPos = new Vector3(CurrentRoadLane.transform.position.x + Settings.Instance.laneSize / 2f, transform.position.y, transform.position.z + 10f);
		GetInputs(verticalInput > 0f, verticalInput < 0f, out float accelerateInput, out float brakeInput);
		avc.ProvideInputs(GetSteering(), accelerateInput, brakeInput);
	}
	
	private void GetInputs(bool accelerate, bool brake, out float accelerateInput, out float brakeInput) {
		brakeInput = 0f;
		if (accelerate) {
			accelerateInput = Mathf.Lerp(1f, 0.5f, Mathf.InverseLerp(defaultSpeed, accelerateSpeed, avc.CurrentSpeed));
			avc.MaxSpeed = accelerateSpeed;
		} else if (brake) {
			avc.MaxSpeed = breakSpeed;
			accelerateInput = Mathf.Lerp(1f, 0.5f, Mathf.InverseLerp(0f, breakSpeed, avc.CurrentSpeed));
			brakeInput = Mathf.InverseLerp(breakSpeed, accelerateSpeed, avc.CurrentSpeed);
		} else {
			accelerateInput = Mathf.Lerp(1f, 0.5f, Mathf.InverseLerp(breakSpeed, defaultSpeed, avc.CurrentSpeed));
			avc.MaxSpeed = defaultSpeed;
		}
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
