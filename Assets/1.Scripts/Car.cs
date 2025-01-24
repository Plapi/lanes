using System;
using UnityEngine;
using ArcadeVP;

public abstract class Car : MonoBehaviour {

	[Space]
	[SerializeField] protected RoadLane roadLane;
	
	[Space]
	[SerializeField] private float requireNewLaneOffset = 100;
	
	[Space]
	[SerializeField] protected ArcadeVehicleController avc;
	
	[Space]
	[SerializeField] protected AnimationCurve steeringCurve;
	[SerializeField] protected float defaultSpeed;
	
	public int RoadLaneIndex { get; private set; }

	private Action onRequireNewLane;
	
	private void Awake() {
		avc.MaxSpeed = defaultSpeed;
	}

	private void OnDisable() {
		avc.carBody.linearVelocity = Vector3.zero;
		avc.carBody.angularVelocity = Vector3.zero;
		avc.rb.linearVelocity = Vector3.zero;
		avc.rb.angularVelocity = Vector3.zero;
		avc.carVelocity = Vector3.zero;
	}

	public void Init(Action onRequireNewLane) {
		this.onRequireNewLane = onRequireNewLane;
	}

	public void SetRoadLane(RoadLane roadLane, int roadLaneIndex) {
		this.roadLane = roadLane;
		RoadLaneIndex = roadLaneIndex;
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
		if (GetRequireNewLanePos().z > roadLane.transform.position.z + roadLane.Length) {
			onRequireNewLane?.Invoke();
		}
	}

	private Vector3 GetTargetPosition() {
		if (roadLane == null) {
			Debug.LogError("No road lane");
		}
		float targetX = roadLane != null ? roadLane.transform.position.x : transform.position.x;
		return new Vector3(targetX + Settings.Instance.laneSize / 2f, transform.position.y, GetTargetPositionZ());
	}

	protected abstract float GetTargetPositionZ();
	
	private Vector3 GetRequireNewLanePos() {
		return transform.position + Vector3.forward * requireNewLaneOffset;
	}
	
#if UNITY_EDITOR
	protected virtual void OnDrawGizmos() {
		if (roadLane == null) {
			return;
		}
		Vector3 targetPosition = GetTargetPosition();
		Gizmos.color = Color.green;
		Gizmos.DrawCube(targetPosition, Vector3.one);
		Gizmos.DrawLine(transform.position, targetPosition);
	}
#endif
}