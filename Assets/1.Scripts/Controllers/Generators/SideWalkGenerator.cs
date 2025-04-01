using UnityEngine;
using System.Collections.Generic;

public class SideWalkGenerator : MonoBehaviour {
 
	[SerializeField] protected MeshFilter[] meshFilters;
	[SerializeField] protected float[] probabilities;
	
	private Mesh[] meshes;
	protected readonly List<Vector3[]> meshVertices = new();
	protected Material[] materials;
	
	private void Awake() {
		if (probabilities == null || probabilities.Length == 0) {
			probabilities = new float[meshFilters.Length];
			for (int i = 0; i < probabilities.Length; i++) {
				probabilities[i] = 0.5f;
			}
		}
		meshes = new Mesh[meshFilters.Length];
		// minBackLeft = new Vector3(float.MaxValue, 0f, float.MaxValue);
		for (int i = 0; i < meshFilters.Length; i++) {
			meshes[i] = meshFilters[i].sharedMesh;
			Vector3[] vertices = new Vector3[meshes[i].vertices.Length];
			for (int j = 0; j < vertices.Length; j++) {
				vertices[j] = meshes[i].vertices[j];
				// if (vertices[j].z < minBackLeft.z) {
				// 	minBackLeft.z = vertices[j].z;
				// }
				// if (vertices[j].x < minBackLeft.x) {
				// 	minBackLeft.x = vertices[j].x;
				// }
			}
			meshVertices.Add(vertices);
		}
		materials = meshFilters[0].GetComponent<MeshRenderer>().materials;
	}

	public void Generate(Vector3 end) {
		
	}
}
