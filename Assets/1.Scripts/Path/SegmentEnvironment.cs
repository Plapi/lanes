using System.Collections.Generic;
using UnityEngine;

public class SegmentEnvironment : MonoBehaviour {

	private readonly List<Element> sideWalks = new();
	private readonly List<Building> buildings = new();

	private Settings.Environment environmentData;
	private int sideWalksLength;
	private int buildingsLength;
	private int unitLength;
	private int xSign;
	private bool isLeftSide;
	private BoxCollider boxCollider;
	
	public void Create(int length, bool isLeftSide) {

		environmentData = Settings.Instance.environment;
		unitLength = Settings.Instance.laneSize;
		buildingsLength = -unitLength;
		this.isLeftSide = isLeftSide;
		xSign = isLeftSide ? -1 : 1;
		
		CreateBoxCollider();
		CreateSideWalks(length);
		CreateBuildings(length);
	}

	public void ContinueGenerateIfNeeded(int length) {
		CreateSideWalks(length);
		CreateBuildings(length);
	}

	private void CreateBoxCollider() {
		boxCollider = new GameObject().AddComponent<BoxCollider>();
		boxCollider.name = "BoxCollider";
		boxCollider.transform.parent = transform;
		boxCollider.transform.localPosition = new Vector3(0f, 0f, -unitLength);
		boxCollider.gameObject.layer = LayerMask.NameToLayer("drivable");
	}

	private void CreateSideWalks(int length) {
		for (int x = isLeftSide ? 0 : unitLength; x <= unitLength * 5; x += unitLength) {
			for (int z = sideWalksLength; z <= length; z += unitLength) {
				sideWalks.Add(environmentData.sideWalk.Create("SideWalk", transform, xSign * x, z));
			}
		}
		sideWalksLength = length + unitLength;
	}

	private void CreateBuildings(int length) {
		int currentZ0 = buildingsLength;
		while (environmentData.TryGetRandomBuilding(length - currentZ0, out Building buildingPrefab)) {
			currentZ0 += buildingPrefab.Length;
			Building building = (Building)buildingPrefab.Create(buildingPrefab.name, transform, 0f, currentZ0);
			if (!isLeftSide) {
				building.transform.SetLocalAngleY(180f);
				building.transform.SetLocalZ(building.transform.localPosition.z - building.Length);
			}
			buildings.Add(building);
		}
		buildingsLength = currentZ0;
		
		/*float space = (float)(length - buildingsLength) / buildings.Count;
		if (space > 0f) {
			float currentZ1 = buildings[0].transform.localPosition.z;
			for (int i = 1; i < buildings.Count; i++) {
				currentZ1 += space + buildings[i].Length;
				buildings[i].transform.SetLocalZ(currentZ1);
			}
		}*/
		
		boxCollider.size = new Vector3(20f, 20f, currentZ0 + unitLength);
		boxCollider.center = new Vector3(xSign * 10f, 10f, boxCollider.size.z / 2f);
	}

	public void Clear() {
		for (int i = 0; i < sideWalks.Count; i++) {
			ObjectPoolManager.Release(sideWalks[i]);
		}
		sideWalks.Clear();
		for (int i = 0; i < buildings.Count; i++) {
			ObjectPoolManager.Release(buildings[i]);
		}
		buildings.Clear();
	}
	
}
