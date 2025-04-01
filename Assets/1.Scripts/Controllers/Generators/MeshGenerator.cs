using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {
    
	[SerializeField] protected MeshFilter[] meshFilters;
	[SerializeField] protected float[] probabilities;
	
	private Mesh[] meshes;
	protected readonly List<Vector3[]> meshVertices = new();
	protected Material[] materials;

	private Vector3 bottomLeftStart;
	private Vector3 bottomRightStart;
	private Vector3 topRightStart;
	private Vector3 topLeftStart;
	
	private void Awake() {
		if (probabilities == null || probabilities.Length == 0) {
			probabilities = new float[meshFilters.Length];
			for (int i = 0; i < probabilities.Length; i++) {
				probabilities[i] = 0.5f;
			}
		}
		meshes = new Mesh[meshFilters.Length];
		for (int i = 0; i < meshFilters.Length; i++) {
			meshes[i] = meshFilters[i].sharedMesh;
			Vector3[] vertices = new Vector3[meshes[i].vertices.Length];
			for (int j = 0; j < vertices.Length; j++) {
				vertices[j] = meshes[i].vertices[j];
			}
			meshVertices.Add(vertices);
		}

		bottomLeftStart = meshes[0].bounds.min;
		topRightStart = meshes[0].bounds.max;
		bottomRightStart = new Vector3(topRightStart.x, 0f, bottomLeftStart.z);
		topLeftStart = new Vector3(bottomLeftStart.x, 0f, topRightStart.z);
		
		materials = meshFilters[0].GetComponent<MeshRenderer>().materials;
	}
	
	public GameObject Generate(string name, Transform parent, int sizeX, int sizeZ) {
		int size = Settings.Instance.laneSize;
		int repetitionsX = Mathf.Abs(sizeX) / size;
		int repetitionsZ = Mathf.Abs(sizeZ) / size;
		
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector2> uvs = new List<Vector2>();
		int vertexOffset = 0;
		
		int dirX = sizeX >= 0 ? 1 : -1;
		int dirZ = sizeZ >= 0 ? 1 : -1;
		Vector3 startOffset = sizeX > 0 && sizeZ > 0 ? bottomLeftStart :
			sizeX < 0 && sizeZ > 0 ? bottomRightStart :
			sizeX > 0 && sizeZ < 0 ? topLeftStart : topRightStart;

		for (int x = 0; x < repetitionsX; x++) {
			for (int z = 0; z < repetitionsZ; z++) {
				Mesh sourceMesh = Utils.SelectRandomItem(meshFilters, probabilities, out int randomIndex).sharedMesh;
				Vector3 offset = new Vector3(x * size * dirX, 0, z * size * dirZ);
				foreach (Vector3 vertex in meshVertices[randomIndex]) {
					vertices.Add(vertex - startOffset + offset);
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

		GameObject go = new GameObject(name) {
			transform = {
				parent = parent
			}
		};
		MeshFilter goMeshFilter = go.AddComponent<MeshFilter>();
		goMeshFilter.mesh = combinedMesh;
		MeshRenderer goMeshRenderer = go.AddComponent<MeshRenderer>();
		goMeshRenderer.materials = materials;
		return go;
	}
}
