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

	private Vector3 prevTargetPointPos;
	private TargetPoint targetPoint;
	
	public string Id {
		get => id;
		set => id = value;
	}
	
	public AICar GetMonoBehaviour() {
		return this;
	}

	private void Awake() {
		prevTargetPointPos = FrontPos;
	}

	public void SetStartPosition(Vector3 pos) {
		transform.position = pos;
		prevTargetPointPos = FrontPos;
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

	private void FixedUpdate() {
		if (targetPoint == null) {
			return;
		}
		
		targetPos = targetPoint.pos;
		targetPos.y = FrontPos.y;
		
		if (targetPoint.AllowPassing()) {
			TargetPoint prevTPoint = null;
			while (targetPoint != null && HasPassedTarget()) {
				prevTPoint = targetPoint;
				targetPoint = null;
				prevTPoint.onReach(this);
			}
			if (prevTPoint != null) {
				prevTargetPointPos = prevTPoint.pos;
				prevTargetPointPos.y = FrontPos.y;
				FixedUpdate();
				return;
			}
		}
		
		Car frontCar = null;
		if (Raycast(FrontPos, transform.forward, 10f, out RaycastHit hit)) {
			frontCar = hit.transform.GetComponent<Car>();
		}
		
		float distToTargetPos = Vector3.Distance(FrontPos, targetPos);
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

	private bool HasPassedTarget() {
		if (targetPoint == null) {
			return false;
		}
		
		Vector2 prev2D = new Vector2(prevTargetPointPos.x, prevTargetPointPos.z);
		Vector2 current2D = new Vector2(FrontPos.x, FrontPos.z);
		Vector2 target2D = new Vector2(targetPoint.pos.x, targetPoint.pos.z);
		
		Vector2 toTargetBefore = target2D - prev2D;
		Vector2 toTargetNow = target2D - current2D;
		bool hasPassedTarget = Vector2.Dot(toTargetBefore.normalized, toTargetNow.normalized) < 0;
		
		return hasPassedTarget;// || Vector2.Distance(current2D, target2D) < 0.1f;
	}

	protected override void OnDrawGizmos() {
		base.OnDrawGizmos();
		if (!drawGizmos || !Application.isPlaying) {
			return;
		}
		Gizmos.color = HasPassedTarget() ? Color.magenta : Color.blue;
		Gizmos.DrawSphere(prevTargetPointPos, gizmosSize);
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
