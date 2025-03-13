using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LaneGenerator : MonoBehaviourSingleton<LaneGenerator> {

	[SerializeField] private LaneElement[] laneElements;

	protected override void Awake() {
		base.Awake();
		for (int i = 0; i < laneElements.Length; i++) {
			laneElements[i].Init();
		}
	}

	public GameObject Generate(Transform parent, LaneData laneData, bool flip = false) {
		LaneElement laneElement = laneElements[(int)laneData.type];
		int laneSize = Settings.Instance.laneSize;
		int repetitions = laneData.length / laneSize;
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector2> uvs = new List<Vector2>();
		int vertexOffset = 0;
		for (int i = 0; i < repetitions; i++) {
			Mesh sourceMesh = Utils
				.SelectRandomItem(laneElement.meshFilters, laneElement.probabilities, out int randomIndex).sharedMesh;
			Vector3 offset = new Vector3(0, 0, i * Settings.Instance.laneSize);
			foreach (Vector3 vertex in laneElement.meshVertices[randomIndex]) {
				vertices.Add(vertex - laneElement.minBackLeft + offset);
			}
			foreach (Vector2 uv in sourceMesh.uv) {
				uvs.Add(uv);
			}
			foreach (int triangle in sourceMesh.triangles) {
				triangles.Add(triangle + vertexOffset);
			}
			vertexOffset += sourceMesh.vertexCount;
		}
		Mesh combinedMesh = new Mesh {
			vertices = vertices.ToArray(),
			triangles = triangles.ToArray(),
			uv = uvs.ToArray()
		};
		combinedMesh.RecalculateNormals();

		// SaveMesh(combinedMesh, laneData.type.ToString());
		
		GameObject go = new GameObject("LaneMesh") {
			transform = {
				parent = parent
			}
		};
		if (flip) {
			go.transform.localPosition = new Vector3(laneSize, 0, laneData.length);
			go.transform.SetAngleY(180f);
		} else {
			go.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		}
		go.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		MeshFilter goMeshFilter = go.AddComponent<MeshFilter>();
		goMeshFilter.mesh = combinedMesh;
		MeshRenderer goMeshRenderer = go.AddComponent<MeshRenderer>();
		goMeshRenderer.materials = laneElement.materials;
		return go;
	}

	[Serializable]
	private class LaneElement {

		public MeshFilter[] meshFilters;
		public float rotationAngle;
		public float[] probabilities;

		public Mesh[] meshes;
		public readonly List<Vector3[]> meshVertices = new();
		public Material[] materials;
		public Vector3 minBackLeft;

		public void Init() {
			if (probabilities == null || probabilities.Length == 0) {
				probabilities = new float[meshFilters.Length];
				for (int i = 0; i < probabilities.Length; i++) {
					probabilities[i] = 0.5f;
				}
			}
			meshes = new Mesh[meshFilters.Length];
			Quaternion rotation = Quaternion.AngleAxis(rotationAngle, Vector3.up);
			minBackLeft = new Vector3(float.MaxValue, 0f, float.MaxValue);
			for (int i = 0; i < meshFilters.Length; i++) {
				meshes[i] = meshFilters[i].sharedMesh;
				Vector3[] vertices = new Vector3[meshes[i].vertices.Length];
				for (int j = 0; j < vertices.Length; j++) {
					vertices[j] = rotation * meshes[i].vertices[j];
					if (vertices[j].z < minBackLeft.z) {
						minBackLeft.z = vertices[j].z;
					}
					if (vertices[j].x < minBackLeft.x) {
						minBackLeft.x = vertices[j].x;
					}
				}
				meshVertices.Add(vertices);
			}
			materials = meshFilters[0].GetComponent<MeshRenderer>().materials;
		}
	}

#if UNITY_EDITOR
	private static void SaveMesh(Mesh mesh, string meshName) {
		const string folderPath = "Assets/2.Prefabs/Roads/FirstSegment/";
		string assetPath = folderPath + meshName + ".asset";
		assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath); // Avoid overwriting existing assets
		AssetDatabase.CreateAsset(mesh, assetPath);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}
#endif
}