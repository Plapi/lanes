#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class MeshPivotChanger : MonoBehaviour {
	
	[SerializeField] private Vector3 pivot;
	[SerializeField] private Mesh originalMesh;
	[SerializeField] private Object objFolder;
	[SerializeField] private string objName;
	
	private void Start() {
		originalMesh = GetComponent<MeshFilter>().sharedMesh;
	}

	[ContextMenu("Change Pivot")]
	private void ChangePivot() {
		Mesh newMesh = Instantiate(originalMesh);
		Vector3 offset = new Vector3(
			Mathf.Lerp(newMesh.bounds.min.x, newMesh.bounds.max.x, pivot.x),
			Mathf.Lerp(newMesh.bounds.min.y, newMesh.bounds.max.y, pivot.y),
			Mathf.Lerp(newMesh.bounds.min.z, newMesh.bounds.max.z, pivot.z)
		);
		Vector3[] vertices = newMesh.vertices;
		for (int i = 0; i < vertices.Length; i++) {
			vertices[i] -= offset;
		}
		newMesh.vertices = vertices;
		newMesh.RecalculateBounds();
		newMesh.RecalculateNormals();
		
		GameObject newObject = new GameObject(gameObject.name + "Mesh") {
			transform = {
				position = transform.position + offset,
				rotation = transform.rotation,
				localScale = transform.localScale
			}
		};
		newObject.AddComponent<MeshFilter>().sharedMesh = newMesh;
		newObject.AddComponent<MeshRenderer>().sharedMaterials = GetComponent<MeshRenderer>().sharedMaterials;
		
		if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(objFolder, out string guid, out long _)) {
			Debug.LogError($"Couldn't find asset GUID: {guid}");
			return;
		}
		string folderPath = AssetDatabase.GUIDToAssetPath(guid);
		MeshFilter meshFilter = newObject.GetComponent<MeshFilter>();
		string meshPath = $"{folderPath}/{objName}.mesh";
		AssetDatabase.CreateAsset(meshFilter.sharedMesh, meshPath);
		meshFilter.sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(meshPath, typeof(Mesh));
		
		PrefabUtility.SaveAsPrefabAsset(newObject, $"{folderPath}/{objName}.prefab").GetComponent<LODGroup>();
		DestroyImmediate(newObject);
	}
}
#endif