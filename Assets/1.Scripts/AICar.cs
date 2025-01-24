using UnityEngine;

public class AICar : Car, IPoolableObject<AICar> {
	
	public RoadLane RoadLane => roadLane;
	
	public string Id { get; set; }
	
	public AICar GetMonoBehaviour() {
		return this;
	}

	protected override void GetInputs(bool accelerate, bool brake, out float accelerateInput, out float brakeInput) {
		accelerateInput = Mathf.Lerp(1f, 0.5f, Mathf.InverseLerp(0f, defaultSpeed, avc.CurrentSpeed));
		brakeInput = 0f;
	}

	protected override float GetTargetPositionZ() {
		return transform.position.z + (roadLane.Data.hasFrontDirection ? 10f : -10f);
	}
}
