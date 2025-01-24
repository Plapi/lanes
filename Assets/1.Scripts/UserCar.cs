using UnityEngine;

public class UserCar : Car {
	
	[Space]
	[SerializeField] private float accelerateSpeed;
	[SerializeField] private float breakSpeed;
	
	[Space]
	[SerializeField] private float releaseSegmentOffset = 10;
	
	protected override void GetInputs(bool accelerate, bool brake, out float accelerateInput, out float brakeInput) {
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

	public Vector3 GetCarReleaseSegmentPos() {
		return transform.localPosition + Vector3.back * releaseSegmentOffset;
	}

/*#if UNITY_EDITOR
	protected override void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Gizmos.DrawCube(GetRequireNewLanePos(), Vector3.one * 0.5f);
		Gizmos.color = Color.red;
		Gizmos.DrawCube(GetCarReleaseSegmentPos(), Vector3.one * 0.5f);
		base.OnDrawGizmos();
	}
#endif*/
}
