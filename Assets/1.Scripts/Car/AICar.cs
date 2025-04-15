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
		if (targetPoint == null) {
			this.targetPoint = null;
			avc.ProvideInputs(0f, 0f, 1f);
			return;
		}
		targetPoint.pos.y = transform.position.y;
		this.targetPoint = targetPoint;
	}

	private void Start() {
		brakeDistanceMin = Random.Range(brakeDistanceMinRange0, brakeDistanceMinRange1);
		meshMaterialRandomizer?.SetRandomMaterial();
	}

	private void Update() {
		if (targetPoint == null) {
			return;
		}
		
		targetPos = targetPoint.pos;
		targetPos.y = FrontPos.y;
		
		float distToTargetPos = Vector3.Distance(FrontPos, targetPos);
		if (targetPoint.AllowPassing() && distToTargetPos < targetPoint.minDistToReach) {
			TargetPoint tempPoint = targetPoint;
			targetPoint = null;
			tempPoint.onReach(this);
			return;
		}

		Car frontCar = null;
		if (Raycast(FrontPos, transform.forward, 10f, out RaycastHit hit)) {
			frontCar = hit.transform.GetComponent<Car>();
		}
		
		float distToNextCar = frontCar != null ? ClosestDistance(frontCar) : float.MaxValue;
		float distToNearestObstacle = targetPoint.AllowPassing() ? distToNextCar : Mathf.Min(distToTargetPos, distToNextCar);
		
		float accelerateInput = Mathf.InverseLerp(3f, 10f, distToNearestObstacle) / 2f;
		float breakInput = Mathf.InverseLerp(3f, brakeDistanceMin, distToNearestObstacle) / 2f;
		avc.ProvideInputs(GetSteering(), accelerateInput, breakInput);
	}

	public bool Raycast(Vector3 origin, Vector3 direction, float distance, out RaycastHit hit) {
		return Physics.Raycast(origin, direction, out hit, distance, raycastLayerMask);
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
		
		if (collision.gameObject.TryGetComponent(out AICar otherCar) && otherCar.targetPoint != null && otherCar.targetPoint.pass) {
			Vector3 dir = otherCar.transform.position - transform.position;
			TargetPoint stopTargetPoint = Vector3.Dot(transform.forward, dir) > 0 ? targetPoint : otherCar.targetPoint;
			if (stopTargetPoint != null) {
				stopTargetPoint.pass = false;
				this.Wait(2f, () => {
					stopTargetPoint.pass = true;
				});	
			}
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
