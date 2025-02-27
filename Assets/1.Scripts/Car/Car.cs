using UnityEngine;
using ArcadeVP;

public abstract class Car : MonoBehaviour {
	
	[Space]
	[SerializeField] protected ArcadeVehicleController avc;
	
	[Space]
	[SerializeField] protected AnimationCurve steeringCurve;
	
	[SerializeField] private BoxCollider boxCollider;

	public BoxCollider BoxCollider => boxCollider;
	
	public Bounds Bounds => boxCollider.bounds;
	public Vector3 FrontPos => transform.position + transform.forward * boxCollider.size.z / 2f - transform.up * boxCollider.size.y * 0.3f;
	public Vector3 BackPos => transform.position - transform.forward * boxCollider.size.z / 2f - transform.up * boxCollider.size.y * 0.3f;
	
	protected Vector3 targetPos;

	private void OnDisable() {
		ResetVelocity();
	}

	private void ResetVelocity() {
		avc.carBody.linearVelocity = Vector3.zero;
		avc.carBody.angularVelocity = Vector3.zero;
		avc.carBody.inertiaTensorRotation = Quaternion.identity;
		avc.carBody.ResetInertiaTensor();
		avc.rb.linearVelocity = Vector3.zero;
		avc.rb.angularVelocity = Vector3.zero;
		avc.rb.inertiaTensorRotation = Quaternion.identity;
		avc.rb.ResetInertiaTensor();
		avc.carVelocity = Vector3.zero;
	}

	public virtual void DisableCar() {
		avc.enabled = false;
		ResetVelocity();
		avc.carBody.isKinematic = true;
		avc.rb.isKinematic = true;
		avc.engineSound.enabled = false;
		avc.SkidSound.enabled = false;
	}

	public virtual void EnableCar() {
		avc.carBody.isKinematic = false;
		avc.rb.isKinematic = false;
		avc.engineSound.enabled = true;
		avc.SkidSound.enabled = true;
		ResetVelocity();
		avc.enabled = true;
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
	
	protected virtual void OnDrawGizmos() {
		if (Application.isPlaying) {
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(targetPos, 0.25f);
			Gizmos.DrawLine(FrontPos, targetPos);
		}
		Gizmos.color = Color.white;
		Gizmos.DrawCube(FrontPos, Vector3.one * 0.2f);
		Gizmos.color = Color.black;
		Gizmos.DrawCube(BackPos, Vector3.one * 0.2f);
	}
}