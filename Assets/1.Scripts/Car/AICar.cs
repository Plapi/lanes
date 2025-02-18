using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class AICar : Car, IPoolableObject<AICar> {
	
	[SerializeField] private string id;
	[SerializeField] private LayerMask raycastLayerMask;
	[SerializeField] private MeshMaterialRandomizer meshMaterialRandomizer;

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
		meshMaterialRandomizer.SetRandomMaterial();
	}

	private void Update() {
		if (targetPoint == null) {
			return;
		}
		
		targetPos = targetPoint.pos;
		
		float distToTargetPos = Vector3.Distance(FrontPos, targetPos);
		if (targetPoint.AllowPassing() && distToTargetPos < targetPoint.minDistToReach) {
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
		
		float distToNextCar = frontCar != null ? ClosestDistance(frontCar) : float.MaxValue;
		float distToNearestObstacle = targetPoint.AllowPassing() ? distToNextCar : Mathf.Min(distToTargetPos, distToNextCar);
		
		float accelerateInput = Mathf.InverseLerp(3f, 10f, distToNearestObstacle) / 2f;
		float breakInput = Mathf.InverseLerp(3f, brakeDistanceMin, distToNearestObstacle) / 2f;
		avc.ProvideInputs(GetSteering(), accelerateInput, breakInput);
	}

	private float ClosestDistance(Car other) {
		Vector3 closestPointOnBox1 = BoxCollider.ClosestPoint(other.transform.position);
		Vector3 closestPointOnBox2 = other.BoxCollider.ClosestPoint(transform.position);
		float distance = Vector3.Distance(closestPointOnBox1, closestPointOnBox2);
		return distance;
	}

	private void OnCollisionEnter(Collision collision) {
		if (targetPoint != null && !targetPoint.pass) {
			return;
		}
		
		if (collision.gameObject.TryGetComponent(out AICar car) && car.targetPoint.pass) {
			targetPoint.pass = false;
			this.Wait(2f, () => {
				targetPoint.pass = true;
			});
		}
	}
}

public class TargetPoint {
	public Vector3 pos;
	public bool pass = true;
	public Func<bool> allowPassing;
	public float minDistToReach = 4f;
	public Action<AICar> onReach;

	public bool AllowPassing() {
		if (!pass) {
			return false;
		}
		return allowPassing == null || allowPassing();
	}
}
