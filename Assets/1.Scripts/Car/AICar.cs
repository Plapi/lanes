using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AICar : Car, IPoolableObject<AICar> {
	
	[SerializeField] private string id;
	[SerializeField] private LayerMask raycastLayerMask;

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
		if (targetPoint.pass && distToTargetPos < targetPoint.minDistToReach) {
			TargetPoint tempPoint = targetPoint;
			targetPoint = null;
			tempPoint.onReach(this);
			return;
		}

		const float rayDistance = 10f;
		Car frontCar = null;
		if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, rayDistance, raycastLayerMask)) {
			frontCar = hit.transform.GetComponent<Car>();
		}
		
		float distToNextCar = frontCar != null ? Vector3.Distance(FrontPos, frontCar.BackPos) : float.MaxValue;
		float distToNearestObstacle = targetPoint.pass ? distToNextCar : Mathf.Min(distToTargetPos, distToNextCar);
		
		float accelerateInput = Mathf.InverseLerp(3f, 10f, distToNearestObstacle) / 2f;
		float breakInput = Mathf.InverseLerp(3f, brakeDistanceMin, distToNearestObstacle) / 2f;
		avc.ProvideInputs(GetSteering(), accelerateInput, breakInput);
	}
}

public class TargetPoint {
	public Vector3 pos;
	public bool pass = true;
	public float minDistToReach = 4f;
	public Action<AICar> onReach;
}
