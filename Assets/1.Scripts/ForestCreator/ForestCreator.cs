#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ForestCreator : MonoBehaviour {

	[SerializeField] private Tree[] trees;
	[SerializeField] private Vector3 min;
	[SerializeField] private Vector3 max;

	[SerializeField] private int treesCount;
	[SerializeField] private List<Tree> instantiatedTrees = new();

	public void CreateForest() {
		ClearForest();

		for (int i = 0; i < treesCount; i++) {
			Tree tree = Instantiate(trees[Random.Range(0, trees.Length)], transform);
			tree.transform.SetLocalAngleY(Random.Range(0, 360f));
			tree.name = tree.name.Replace("(Clone)", string.Empty);

			List<Vector3> possiblePositions = new List<Vector3>();

			for (float x = min.x + tree.Radius / 2f; x <= max.x - tree.Radius / 2f; x += 0.1f) {
				for (float z = min.z + tree.Radius / 2f; z <= max.z - tree.Radius / 2f; z += 0.1f) {
					Vector3 position = new Vector3(x, 0f, z);
					if (IsPossiblePosition(tree, position)) {
						possiblePositions.Add(position);
					}
				}
			}

			if (possiblePositions.Count == 0) {
				Debug.LogError($"No possible position found for {tree.name} at {i}");
				DestroyImmediate(tree.gameObject);
				return;
			}

			tree.transform.localPosition = possiblePositions[Random.Range(0, possiblePositions.Count)];
			instantiatedTrees.Add(tree);
		}
	}

	public void CreateMeshLod() {
		/*if (!TryGetComponent(out MeshLODCreator meshLODCreator)) {
			meshLODCreator = gameObject.AddComponent<MeshLODCreator>();
		}

		string folderPaths = $"Assets/EnvLODS/Forests/{m_name}";
		int fIndex = 0;
		while (AssetDatabase.IsValidFolder($"{folderPaths}/Forest{fIndex}")) {
			fIndex++;
		}
		string guid = AssetDatabase.CreateFolder(folderPaths, $"Forest{fIndex}");
		string folderPath = AssetDatabase.GUIDToAssetPath(guid);

		LODGroup lodGroup = meshLODCreator.Create()[0];
		LOD[] lods = lodGroup.GetLODs();

		for (int i = 0; i < lods.Length; i++) {
			Renderer[] rends = lods[i].renderers;
			for (int k = 0; k < rends.Length; k++) {
				MeshFilter meshFilter = rends[k].GetComponent<MeshFilter>();
				string meshPath = $"{folderPath}/mesh{i}{k}.mesh";
				AssetDatabase.CreateAsset(meshFilter.sharedMesh, meshPath);
				meshFilter.sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(meshPath, typeof(Mesh));
			}
		}

		List<(Vector3, float)> treesData = new List<(Vector3, float)>(m_instantiatedTrees.Count);
		m_instantiatedTrees.ForEach(tree => {
			treesData.Add((tree.transform.localPosition, tree.radius));
		});
		lodGroup.gameObject.AddComponent<ForestLOD>().Init(treesData);

		GameObject obj = lodGroup.gameObject;
		PrefabUtility.SaveAsPrefabAsset(lodGroup.gameObject, $"{folderPath}/Forest{fIndex}.prefab").GetComponent<LODGroup>();
		DestroyImmediate(obj);*/
	}

	public void ClearForest() {
		instantiatedTrees.ForEach(tree => DestroyImmediate(tree.gameObject));
		instantiatedTrees.Clear();
	}

	private bool IsPossiblePosition(Tree tree, Vector3 position) {
		foreach (Tree instantiatedTree in instantiatedTrees) {
			float distance = Vector3.Distance(position, instantiatedTree.transform.localPosition);
			float minDistance = tree.Radius + instantiatedTree.Radius;
			if (distance < minDistance) {
				return false;
			}
		}
		return true;
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position + min, transform.position + new Vector3(min.x, 0f, max.z));
		Gizmos.DrawLine(transform.position + new Vector3(min.x, 0f, max.z), transform.position + max);
		Gizmos.DrawLine(transform.position + max, transform.position + new Vector3(max.x, 0f, min.z));
		Gizmos.DrawLine(transform.position + new Vector3(max.x, 0f, min.z), transform.position + min);
	}
}

[CustomEditor(typeof(ForestCreator))]
public class ForestCreatorEditor : Editor {
	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		if (GUILayout.Button("Create Forest")) {
			((ForestCreator)target).CreateForest();
		}
		if (GUILayout.Button("Create Mesh LOD")) {
			((ForestCreator)target).CreateMeshLod();
		}
		if (GUILayout.Button("Clear Forest")) {
			((ForestCreator)target).ClearForest();
		}

	}
}
#endif
