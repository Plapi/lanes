using System.Collections.Generic;
using UnityEngine;

public class SegmentEnvironment : MonoBehaviour {

	private readonly List<Building> buildings = new();

	private Transform leftSide;
	private Transform rightSide;
	
	private int leftBuildingsLength;
	private int rightBuildingsLength;
	
	private BoxCollider leftBoxCollider;
	private BoxCollider rightBoxCollider;

	private const int sideWalkWidth = 25;
	private GameObject leftSideWalkMesh0;
	private GameObject leftSideWalkMesh1;
	private GameObject rightSideWalkMesh0;
	private GameObject rightSideWalkMesh1;
	private int leftSideWalkLength;
	private int rightSideWalkLength;
	
	public void Create() {
		
	}
	
	public void Create(int lengthLeft0, int lengthRight0, int lengthLeft1, int lengthRight1, int laneWidth) {
		// leftSide = CreateSide("LeftSide");
		// rightSide = CreateSide("RightSide");
		// rightSide.transform.SetLocalX(laneWidth);
		//
		// leftBoxCollider = CreateBoxCollider(leftSide);
		// rightBoxCollider = CreateBoxCollider(rightSide);
		//
		// CreateSideWalks(lengthLeft0, lengthRight0, lengthLeft1, lengthRight1);
		// CreateBuildings(lengthLeft, lengthRight);
	}

	public void ContinueCreateIfNeeded(int leftMaxZ, int rightMaxZ) {
		// ContinueSideWalksIfNeeded(leftMaxZ, rightMaxZ);
		// CreateBuildings(leftMaxZ - (int)leftSide.transform.position.z, rightMaxZ - (int)rightSide.transform.position.z);
	}
	
	private void CreateSideWalks(int leftLength0, int rightLength0, int leftLength1, int rightLength1) {
		leftSideWalkMesh0 = GeneratorsController.Instance.EnvironmentGenerator.GenerateSideWalk(leftSide, leftLength0, sideWalkWidth);
		leftSideWalkMesh0.transform.SetLocalX(-sideWalkWidth);
		
		rightSideWalkMesh0 = leftLength0 == rightLength0 ? Instantiate(leftSideWalkMesh0, rightSide) : 
			GeneratorsController.Instance.EnvironmentGenerator.GenerateSideWalk(rightSide, rightLength0, sideWalkWidth);
		rightSideWalkMesh0.transform.SetLocalX(0f);
		
		leftSideWalkMesh1 = GeneratorsController.Instance.EnvironmentGenerator.GenerateSideWalk(leftSide, leftLength1 - sideWalkWidth, sideWalkWidth);
		leftSideWalkMesh1.transform.SetLocalXZ(-sideWalkWidth, leftLength0 - sideWalkWidth);
		leftSideWalkMesh1.transform.SetLocalAngleY(-90f);
		
		rightSideWalkMesh1 = GeneratorsController.Instance.EnvironmentGenerator.GenerateSideWalk(rightSide, rightLength1 - sideWalkWidth, sideWalkWidth);
		rightSideWalkMesh1.transform.SetLocalXZ(sideWalkWidth, rightLength0);
		rightSideWalkMesh1.transform.SetLocalAngleY(90f);
		
		leftBoxCollider.size = new Vector3(leftLength1, 20f, leftLength0);
		leftBoxCollider.center = new Vector3(-leftLength1 / 2f, 10f, leftLength0 / 2f);
		
		rightBoxCollider.size = new Vector3(rightLength1, 20f, rightLength0);
		rightBoxCollider.center = new Vector3(rightLength1 / 2f, 10f, rightLength0 / 2f);
		
		// leftSideWalkLength = lengthLeft0;
		// rightSideWalkLength = lengthRight0;
	}

	private void ContinueSideWalksIfNeeded(int leftMaxZ, int rightMaxZ) {
		int leftCurrentMaxZ = (int)leftSideWalkMesh0.transform.position.z + leftSideWalkLength;
		int rightCurrentMaxZ = (int)rightSideWalkMesh0.transform.position.z + rightSideWalkLength;

		int leftDif = leftMaxZ - leftCurrentMaxZ;
		int rightDif = rightMaxZ - rightCurrentMaxZ;
		if (leftDif > 0) {
			GameObject leftSWMesh = GeneratorsController.Instance.EnvironmentGenerator.GenerateSideWalk(leftSide, leftDif, sideWalkWidth);
			leftSWMesh.transform.SetLocalX(-sideWalkWidth);
			leftSWMesh.SetZ(leftCurrentMaxZ);
		}
		if (rightDif > 0) {
			GameObject rightSWMesh = leftDif == rightDif ? Instantiate(rightSideWalkMesh0, transform) :
				GeneratorsController.Instance.EnvironmentGenerator.GenerateSideWalk(rightSide, rightDif, sideWalkWidth);
			rightSWMesh.transform.SetLocalX(0f);
			rightSWMesh.SetZ(rightCurrentMaxZ);
		}
	}

	public void SetZ(int leftZ, int rightZ) {
		leftSide.SetZ(leftZ);
		rightSide.SetZ(rightZ);
	}

	private void CreateBuildings(int lengthLeft, int lengthRight) {
		leftBuildingsLength = CreateBuildings(leftBuildingsLength, lengthLeft, true, leftBoxCollider, leftSide);
		rightBuildingsLength = CreateBuildings(rightBuildingsLength, lengthRight, false, rightBoxCollider, rightSide);
	}
	
	private int CreateBuildings(int lengthStart, int length, bool isLeftSide, BoxCollider boxCollider,  Transform parent) {
		int currentZ0 = lengthStart;
		
		while (GeneratorsController.Instance.EnvironmentGenerator.TryGetRandomBuilding(length - currentZ0, out Building buildingPrefab)) {
			currentZ0 += buildingPrefab.Length;
			Building building = (Building)buildingPrefab.Create(buildingPrefab.name, parent, 0f, currentZ0);
			if (!isLeftSide) {
				building.transform.SetLocalAngleY(180f);
				building.transform.SetLocalZ(building.transform.localPosition.z - building.Length);
			}
			buildings.Add(building);
		}

		boxCollider.size = new Vector3(20f, 20f, currentZ0);
		boxCollider.center = new Vector3((isLeftSide ? -1 : 1) * 10f, 10f, boxCollider.size.z / 2f);
		
		return currentZ0;
	}
	
	private static BoxCollider CreateBoxCollider(Transform parent) {
		BoxCollider boxCollider = new GameObject("BoxCollider").AddComponent<BoxCollider>();
		boxCollider.transform.parent = parent;
		boxCollider.transform.localPosition = Vector3.zero;
		boxCollider.gameObject.layer = LayerMask.NameToLayer("drivable");
		return boxCollider;
	}

	private Transform CreateSide(string name) {
		Transform side = new GameObject(name).transform;
		side.transform.parent = transform;
		side.transform.localPosition = Vector3.zero;
		return side;
	}

	public void Clear() {
		for (int i = 0; i < buildings.Count; i++) {
			ObjectPoolManager.Release(buildings[i]);
		}
		buildings.Clear();
	}
	
}
