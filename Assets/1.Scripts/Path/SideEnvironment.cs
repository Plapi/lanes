using System.Collections.Generic;
using UnityEngine;

public class SideEnvironment : MonoBehaviour {

	private static int boxColliderLayer = -1;

	private readonly List<Building> buildings = new();
	
	public void Generate(Vector3 end, bool isLeftSide) {
		CreateSideWalk(end);
		CreateBuildings(end, isLeftSide);
		CreateBoxCollider(end);
	}

	private void CreateSideWalk(Vector3 end) {
		GameObject sideWalk = GeneratorsController.Instance.SideWalkGenerator.Generate("SideWalk",  transform,
			Mathf.RoundToInt(end.x - transform.position.x), Mathf.RoundToInt(end.z - transform.position.z));
		sideWalk.transform.localPosition = Vector3.zero;
	}

	private void CreateBuildings(Vector3 end, bool isLeftSide) {
		
		Utils.GetIntersection(transform.position, -transform.forward, end, transform.right, out Vector3 intersection);
		Vector3 dir = (intersection - transform.position).normalized;
		float angleY = isLeftSide ? 0f : 180f;

		int length = Mathf.RoundToInt(Vector3.Distance(transform.position, intersection));
		float currentDist = 0f;
		while (currentDist <= length) {
			if (!GeneratorsController.Instance.EnvironmentGenerator.TryGetRandomBuilding(length - Mathf.RoundToInt(currentDist), out Building buildingPrefab)) {
				break;
			}
			
			currentDist += buildingPrefab.Length / 2f;
			Building building = (Building)buildingPrefab.Create(buildingPrefab.name, transform, 0f);
			building.transform.SetLocalAngleY(angleY);
			building.transform.position = transform.position + dir * currentDist;
			buildings.Add(building);
			currentDist += buildingPrefab.Length / 2f;
			
		}
	}

	private void CreateBoxCollider(Vector3 end) {
		BoxCollider boxCollider = new GameObject("BoxCollider").AddComponent<BoxCollider>();
		boxCollider.transform.parent = transform;
		boxCollider.transform.localPosition = Vector3.zero;
		boxCollider.gameObject.layer = boxColliderLayer == -1 ? boxColliderLayer = LayerMask.NameToLayer("drivable") : boxColliderLayer;
		Vector3 pos0 = transform.position;
		float centerX = (pos0.x + end.x) / 2f;
		float centerZ = (pos0.z + end.z) / 2f;
		boxCollider.transform.position = new Vector3(centerX, 10f, centerZ);
		boxCollider.center = Vector3.zero;
		boxCollider.size = new Vector3(Mathf.Abs(end.x - pos0.x), 20f, Mathf.Abs(end.z - pos0.z));
	}

	public void Clear() {
		for (int i = 0; i < buildings.Count; i++) {
			ObjectPoolManager.Release(buildings[i]);
		}
		Destroy(gameObject);
	}

	private GameObject t0;
	private GameObject t1;
	private void OnDrawGizmos() {
		if (t0 != null && t1 != null) {
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(t0.transform.position, t1.transform.position);	
		}
	}
}
