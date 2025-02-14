using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AICar : Car, IPoolableObject<AICar> {
	
	[SerializeField] private string id;

	[SerializeField] private AICar nextCar;

	private const float brakeDistanceMinRange0 = 0.8f;
	private const float brakeDistanceMinRange1 = 2f;

	private float brakeDistanceMin;

	private TargetPoint targetPoint;
	
	public string Id {
		get => id;
		set => id = value;
	}
	
	public AICar GetMonoBehaviour() {
		return this;
	}

	public void SetNextCar(AICar nextCar) {
		this.nextCar = nextCar;
	}

	public void SetTargetPoint(TargetPoint targetPoint) {
		targetPoint.pos.y = transform.position.y;
		this.targetPoint = targetPoint;
	}

	private void Start() {
		brakeDistanceMin = Random.Range(brakeDistanceMinRange0, brakeDistanceMinRange1);
	}

	private void Update() {
		if (targetPoint == null) {
			return;
		}
		
		targetPos = targetPoint.pos;
		
		float distToTargetPos = Vector3.Distance(FrontPos, targetPos);
		if (distToTargetPos < targetPoint.minDistToReach) {
			TargetPoint tempPoint = targetPoint;
			targetPoint = null;
			tempPoint.onReach(this);
			return;
		}
		
		float distToNextCar = nextCar != null ? Vector3.Distance(FrontPos, nextCar.BackPos) : float.MaxValue;
		float distToNearestObstacle = targetPoint.pass ? distToNextCar : Mathf.Min(distToTargetPos, distToNextCar);
		
		float accelerateInput = Mathf.InverseLerp(3f, 10f, distToNearestObstacle) / 2f;
		float breakInput = Mathf.InverseLerp(3f, brakeDistanceMin, distToNearestObstacle) / 2f;
		avc.ProvideInputs(GetSteering(), accelerateInput, breakInput);
	}

	/*protected override void GetInputs(bool accelerate, bool brake, out float accelerateInput, out float brakeInput) {
		accelerateInput = Mathf.Lerp(1f, 0.5f, Mathf.InverseLerp(0f, defaultSpeed, avc.CurrentSpeed));
		brakeInput = 0f;
	}

	protected override float GetTargetPositionZ() {
		return transform.position.z + (CurrentRoadLane.Data.hasFrontDirection ? 10f : -10f);
	}*/
}

public class TargetPoint {
	public Vector3 pos;
	public bool pass = true;
	public float minDistToReach = 4f;
	public Action<AICar> onReach;
}
