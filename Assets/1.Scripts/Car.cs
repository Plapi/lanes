using UnityEngine;
using ArcadeVP;

public class Car : MonoBehaviour {

	[SerializeField] private RoadLane roadLane;
	
	[Space]
	[SerializeField] private ArcadeVehicleController avc;
	[SerializeField] private AnimationCurve steeringCurve;
	
	[Space]
	[SerializeField] private float defaultSpeed;
	[SerializeField] private float accelerateSpeed;
	[SerializeField] private float breakSpeed;

	private void Awake() {
		avc.MaxSpeed = defaultSpeed;
	}

	public void SetRoadLane(RoadLane roadLane) {
		this.roadLane = roadLane;
	}

	public void UpdateCar(bool accelerate, bool brake) {
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

		float accelerateInput;
		float brakeInput = 0f;
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

		avc.ProvideInputs(steering, accelerateInput, brakeInput);
	}

	private Vector3 GetTargetPosition() {
		float targetX = roadLane != null ? roadLane.transform.position.x : transform.position.x;
		return new Vector3(targetX + Settings.Instance.laneSize / 2f, transform.position.y, transform.position.z + 10f);
	}
	
#if UNITY_EDITOR
	private void OnDrawGizmos() {
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