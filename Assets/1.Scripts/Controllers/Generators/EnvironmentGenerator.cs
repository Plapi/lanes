using System.Collections.Generic;
using UnityEngine;

public class EnvironmentGenerator : MonoBehaviour {
    
	[SerializeField] private MeshFilter[] meshFiltersSideWalks;
	[SerializeField] private float[] probabilitiesSideWalks;
	
	[Space]
	[SerializeField] private Building[] buildings;
	[SerializeField] [Range(0f, 1f)] private float[] buildingRandomProbabilities;
	
	private Mesh[] meshesSideWalks;
	private readonly List<Vector3[]> meshVerticesSideWalks = new();
	private Material[] materialsSideWalks;
	private Vector3 minBackLeftSideWalk;
	
	private void Awake() {
		SetupSideWalk();
	}
	
	public bool TryGetRandomBuilding(int maxLength, out Building building) {
		List<Building> list = new();
		List<float> probabilities = new();
		for (int i = 0; i < buildings.Length; i++) {
			if (buildings[i].Length <= maxLength) {
				list.Add(buildings[i]);
				probabilities.Add(buildingRandomProbabilities[i]);
			}
		}
		building = list.Count > 0 ? Utils.SelectRandomItem(list.ToArray(), probabilities.ToArray(), out _): null;
		return building != null;
	}

	private void SetupSideWalk() {
		if (probabilitiesSideWalks == null || probabilitiesSideWalks.Length == 0) {
			probabilitiesSideWalks = new float[meshFiltersSideWalks.Length];
			for (int i = 0; i < probabilitiesSideWalks.Length; i++) {
				probabilitiesSideWalks[i] = 0.5f;
			}
		}
		meshesSideWalks = new Mesh[meshFiltersSideWalks.Length];
		minBackLeftSideWalk = new Vector3(float.MaxValue, 0f, float.MaxValue);
		for (int i = 0; i < meshFiltersSideWalks.Length; i++) {
			meshesSideWalks[i] = meshFiltersSideWalks[i].sharedMesh;
			Vector3[] vertices = new Vector3[meshesSideWalks[i].vertices.Length];
			for (int j = 0; j < vertices.Length; j++) {
				vertices[j] = meshesSideWalks[i].vertices[j];
				if (vertices[j].z < minBackLeftSideWalk.z) {
					minBackLeftSideWalk.z = vertices[j].z;
				}
				if (vertices[j].x < minBackLeftSideWalk.x) {
					minBackLeftSideWalk.x = vertices[j].x;
				}
			}
			meshVerticesSideWalks.Add(vertices);
		}
		materialsSideWalks = meshFiltersSideWalks[0].GetComponent<MeshRenderer>().materials;
	}

	public GameObject GenerateSideWalk(Transform parent, int length, int width) {
		int size = Settings.Instance.laneSize;
		int repetitionsX = width / size;
		int repetitionsZ = length / size;
		
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector2> uvs = new List<Vector2>();
		int vertexOffset = 0;
		for (int x = 0; x < repetitionsX; x++) {
			for (int z = 0; z < repetitionsZ; z++) {
				Mesh sourceMesh = Utils.SelectRandomItem(meshFiltersSideWalks, probabilitiesSideWalks, out int randomIndex).sharedMesh;
				Vector3 offset = new Vector3(x * size, 0, z * size);
				foreach (Vector3 vertex in meshVerticesSideWalks[randomIndex]) {
					vertices.Add(vertex - minBackLeftSideWalk + offset);
				}
				foreach (Vector2 uv in sourceMesh.uv) {
					uvs.Add(uv);
				}
				foreach (int triangle in sourceMesh.triangles) {
					triangles.Add(triangle + vertexOffset);
				}
				vertexOffset += sourceMesh.vertexCount;
			}
		}
		Mesh combinedMesh = new Mesh {
			vertices = vertices.ToArray(),
			triangles = triangles.ToArray(),
			uv = uvs.ToArray()
		};
		combinedMesh.RecalculateNormals();

		GameObject go = new GameObject("SideWalk") {
			transform = {
				parent = parent
			}
		};
		go.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		MeshFilter goMeshFilter = go.AddComponent<MeshFilter>();
		goMeshFilter.mesh = combinedMesh;
		MeshRenderer goMeshRenderer = go.AddComponent<MeshRenderer>();
		goMeshRenderer.materials = materialsSideWalks;
		return go;
	}
}
