using UnityEngine;
using ArcadeVP;

public abstract class Car : MonoBehaviour {
	
	[Space]
	[SerializeField] protected ArcadeVehicleController avc;
	
	[Space]
	[SerializeField] protected AnimationCurve steeringCurve;
	[SerializeField] protected float defaultSpeed;
	
	public int RoadLaneIndex { get; private set; }
	public Segment CurrentSegment { get; private set; }
	
	public RoadLane CurrentRoadLane => CurrentSegment.RoadLanes[RoadLaneIndex];
	
	private void Awake() {
		avc.MaxSpeed = defaultSpeed;
	}

	private void OnDisable() {
		avc.carBody.linearVelocity = Vector3.zero;
		avc.carBody.angularVelocity = Vector3.zero;
		avc.rb.linearVelocity = Vector3.zero;
		avc.rb.angularVelocity = Vector3.zero;
		avc.carVelocity = Vector3.zero;
		RoadLaneIndex = 0;
		CurrentSegment = null;
	}

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
	
	public void TrySwitchLane(int add) {
		int newLaneIndex = RoadLaneIndex + add;
		if (newLaneIndex >= 0 && newLaneIndex < CurrentSegment.RoadLanes.Count) {
			RoadLaneIndex = newLaneIndex;
		}
	}

	protected abstract void GetInputs(bool accelerate, bool brake, out float accelerateInput, out float brakeInput);

	private float GetSteering() {
		Vector3 targetPosition = GetTargetPosition();

		Vector3 targetDir = targetPosition - transform.position;
		float signedAngle = Vector3.SignedAngle(targetDir, transform.forward, Vector3.up);
		float angle = Mathf.Abs(signedAngle);
		
		float steering = 0f;
		if (angle > Mathf.Epsilon) {
			float angleP = Mathf.InverseLerp(0, 45, angle);
			steering = steeringCurve.Evaluate(angleP);
			steering *= -Mathf.Sign(signedAngle);
		}

		return steering;
	}

	public void UpdateCar(float verticalInput) {
		GetInputs(verticalInput > 0f, verticalInput < 0f, out float accelerateInput, out float brakeInput);
		avc.ProvideInputs(GetSteering(), accelerateInput, brakeInput);
	}

	private Vector3 GetTargetPosition() {
		float targetX = CurrentRoadLane != null ? CurrentRoadLane.transform.position.x : transform.position.x;
		return new Vector3(targetX + Settings.Instance.laneSize / 2f, transform.position.y, GetTargetPositionZ());
	}

	protected abstract float GetTargetPositionZ();
	
#if UNITY_EDITOR
	protected virtual void OnDrawGizmos() {
		Vector3 targetPosition = GetTargetPosition();
		Gizmos.color = Color.green;
		Gizmos.DrawCube(targetPosition, Vector3.one);
		Gizmos.DrawLine(transform.position, targetPosition);
	}
#endif
}