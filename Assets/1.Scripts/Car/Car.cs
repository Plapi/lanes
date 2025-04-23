using UnityEngine;
using ArcadeVP;
using Com.ForbiddenByte.OSA.Core;

public abstract class Car : MonoBehaviour {
	
	[Space]
	[SerializeField] protected ArcadeVehicleController avc;
	
	[Space]
	[SerializeField] protected AnimationCurve steeringCurve;
	
	[SerializeField] private BoxCollider boxCollider;

	public BoxCollider BoxCollider => boxCollider;

	public float MaxSpeed {
		get => avc.MaxSpeed;
		set => avc.MaxSpeed = value;
	}
	
	public Bounds Bounds => boxCollider.bounds;
	public Vector3 FrontPos => transform.position + transform.forward * boxCollider.size.z / 2f - transform.up * boxCollider.size.y * 0.3f;
	public Vector3 BackPos => transform.position - transform.forward * boxCollider.size.z / 2f - transform.up * boxCollider.size.y * 0.3f;
	
	protected Vector3 targetPos;

	private void OnDisable() {
		ResetVelocity();
	}

	private void ResetVelocity() {
		if (avc == null) {
			return;
		}
		if (!avc.carBody.isKinematic) {
			avc.carBody.linearVelocity = Vector3.zero;
			avc.carBody.angularVelocity = Vector3.zero;
		}
		avc.carBody.inertiaTensorRotation = Quaternion.identity;
		avc.carBody.ResetInertiaTensor();
		if (!avc.rb.isKinematic) {
			avc.rb.linearVelocity = Vector3.zero;
			avc.rb.angularVelocity = Vector3.zero;
		}
		avc.rb.inertiaTensorRotation = Quaternion.identity;
		avc.rb.ResetInertiaTensor();
		avc.carVelocity = Vector3.zero;
	}

	public virtual void DisableCar() {
		avc.enabled = false;
		ResetVelocity();
		avc.carBody.isKinematic = true;
		avc.rb.isKinematic = true;
		SetSoundEnabled(false);
		SetSoundEnabled(false);
	}

	public virtual void EnableCar() {
		avc.carBody.isKinematic = false;
		avc.rb.isKinematic = false;
		SetSoundEnabled(true);
		ResetVelocity();
		avc.enabled = true;
		SetSoundEnabled(true);
	}

	public virtual void SetSoundEnabled(bool enabled) {
		if (avc.SkidSound != null) {
			avc.SkidSound.enabled = enabled;	
		}
	}

	protected float GetSteering() {
		Vector3 fPos = FrontPos -transform.forward * 1f;
		// GizmosController.Instance.DrawSphere("120", fPos, 0.1f, Color.magenta);
		
		Vector3 targetDir = targetPos - fPos;
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
	
	[Space]
	[SerializeField] protected bool drawGizmos;
	[SerializeField] protected float gizmosSize = 0.1f;
	protected virtual void OnDrawGizmos() {
		if (!drawGizmos) {
			return;
		}
		if (Application.isPlaying) {
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(targetPos, gizmosSize);
			Gizmos.DrawLine(FrontPos, targetPos);
		}
		Gizmos.color = Color.white;
		Gizmos.DrawSphere(FrontPos, gizmosSize);
		Gizmos.color = Color.black;
		Gizmos.DrawSphere(BackPos, gizmosSize);
	}
}