using UnityEngine;
using ArcadeVP;

public abstract class Car : MonoBehaviour {
	
	[Space]
	[SerializeField] protected ArcadeVehicleController avc;
	
	[Space]
	[SerializeField] protected AnimationCurve steeringCurve;
	[SerializeField] protected float defaultSpeed;
	
	[SerializeField] private BoxCollider boxCollider;
	
	public BoxCollider BoxCollider => boxCollider;
	
	public Bounds Bounds => boxCollider.bounds;
	public Vector3 FrontPos => transform.position + transform.forward * boxCollider.size.z / 2f;
	public Vector3 BackPos => transform.position - transform.forward * boxCollider.size.z / 2f;
	
	public int RoadLaneIndex { get; private set; }
	public Segment CurrentSegment { get; private set; }
	
	public RoadLane CurrentRoadLane => CurrentSegment.RoadLanes[RoadLaneIndex];
	
	protected Vector3 targetPos;
	private Vector3 steeringTargetPos;
	
	private void Awake() {
		avc.MaxSpeed = defaultSpeed;
	}

	private void OnDisable() {
		avc.carBody.linearVelocity = Vector3.zero;
		avc.carBody.angularVelocity = Vector3.zero;
		avc.carBody.inertiaTensorRotation = Quaternion.identity;
		avc.carBody.ResetInertiaTensor();
		avc.rb.linearVelocity = Vector3.zero;
		avc.rb.angularVelocity = Vector3.zero;
		avc.rb.inertiaTensorRotation = Quaternion.identity;
		avc.rb.ResetInertiaTensor();
		avc.carVelocity = Vector3.zero;
		RoadLaneIndex = 0;
		CurrentSegment = null;
	}
	
	public void SetTargetPos(Vector3 targetPos) {
		this.targetPos = targetPos;
		this.targetPos.y = transform.position.y;
	}

	protected float GetSteering() {
		Vector3 targetDir = targetPos - FrontPos;
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
	
	/*public void SetSegment(Segment segment, int laneIndex) {
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
*/
	protected virtual void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(targetPos, 0.25f);
		Gizmos.DrawLine(FrontPos, targetPos);
		Gizmos.color = Color.white;
		Gizmos.DrawCube(FrontPos, Vector3.one * 0.2f);
		Gizmos.color = Color.black;
		Gizmos.DrawCube(BackPos, Vector3.one * 0.2f);
	}
}