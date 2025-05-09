#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MeshCombiner : MonoBehaviour {

	private const int MAX_VERTICES = 65535;
	
	[SerializeField] private Object objFolder;
	[SerializeField] private string objName;
	
	[Space]
	[SerializeField] [Range(0f, 1f)] private float quality = 0.5f;

	public void SetOutput(Object objFolder, string objName) {
		this.objFolder = objFolder;
		this.objName = objName;
	}
	
	public void Combine(Transform tr, System.Action<GameObject> onObjCreated = null) {
		Vector3 prevPos = transform.position;
		Quaternion prevRot = transform.rotation;
		transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
		
		GameObject obj = MeshUtils.Combine(tr);
		
		if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(objFolder, out string guid, out long _)) {
			Debug.LogError($"Couldn't find asset GUID: {guid}");
			return;
		}
		
		string folderPath = AssetDatabase.GUIDToAssetPath(guid);
		MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
		string meshPath = $"{folderPath}/{objName}.mesh";
		AssetDatabase.CreateAsset(meshFilter.sharedMesh, meshPath);
		meshFilter.sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(meshPath, typeof(Mesh));
		
		onObjCreated?.Invoke(obj);
		
		PrefabUtility.SaveAsPrefabAsset(obj, $"{folderPath}/{objName}.prefab").GetComponent<LODGroup>();
		DestroyImmediate(obj);
		
		transform.SetPositionAndRotation(prevPos, prevRot);
	}

	private void CombineOnParts() {
		
		MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
		int currentVertexCount = 0;
		int currentPart = 0;
		List<Transform> children = new();
		List<GameObject> parts = new();
		
		void createObjPart() {
			GameObject part = new GameObject($"Part{currentPart}") {
				transform = {
					parent = transform,
					localPosition = Vector3.zero
				}
			};
			foreach (var child in children) {
				child.transform.parent = part.transform;
			}
			children.Clear();
			currentVertexCount = 0;
			currentPart++;
			parts.Add(part);
		}
		
		for (int i = 0; i < meshFilters.Length; i++) {
			
			int vertexCount = meshFilters[i].sharedMesh.vertexCount;
			if (vertexCount + currentVertexCount > MAX_VERTICES) {
				createObjPart();
			}
			
			children.Add(meshFilters[i].transform);

			if (i == meshFilters.Length - 1) {
				createObjPart();
			}

			currentVertexCount += vertexCount;
		}

		string prevName = objName;
		for (int i = 0; i < parts.Count; i++) {
			objName = $"{prevName}Part{i}";
			Combine(parts[i].transform);
		}
		objName = prevName;
	}

	private void VerticesReduction() {
		GameObject obj = MeshUtils.OptimizeChildMeshes(transform, quality).gameObject;
	}

	[CustomEditor(typeof(MeshCombiner))]
	private class MeshCombinerEditor : Editor {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			MeshCombiner meshCombiner = (MeshCombiner)target;
			GUILayout.Space(10f);
			if (GUILayout.Button("Combine")) {
				meshCombiner.Combine(meshCombiner.transform);
			}
			if (GUILayout.Button("Combine On Parts")) {
				meshCombiner.CombineOnParts();
			}
			if (GUILayout.Button("Vertices Reduction")) {
				meshCombiner.VerticesReduction();
			}
		}
	}
}

#endif