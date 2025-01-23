using UnityEngine;

public class AICar : Car {
	
	public RoadLane RoadLane => roadLane;
	
	protected override void GetInputs(bool accelerate, bool brake, out float accelerateInput, out float brakeInput) {
		accelerateInput = Mathf.Lerp(1f, 0.5f, Mathf.InverseLerp(0f, defaultSpeed, avc.CurrentSpeed));
		brakeInput = 0f;
	}
}
