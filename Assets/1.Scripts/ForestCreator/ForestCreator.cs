#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;

public class ForestCreator : MonoBehaviour {

	[SerializeField] private ForestObject[] forestObjects;
	[SerializeField] private Vector3 min;
	[SerializeField] private Vector3 max;

	[SerializeField] private int forestObjectsCount;
	[SerializeField] private List<ForestObject> instantiatedForestObjects = new();

	public void CreateForest() {
		ClearForest();

		for (int i = 0; i < forestObjectsCount; i++) {
			ForestObject forestObject = Instantiate(forestObjects[Random.Range(0, forestObjects.Length)], transform);
			forestObject.transform.SetLocalAngleY(Random.Range(0, 360f));
			forestObject.name = forestObject.name.Replace("(Clone)", string.Empty);

			List<Vector3> possiblePositions = new List<Vector3>();

			for (float x = min.x + forestObject.Radius / 2f; x <= max.x - forestObject.Radius / 2f; x += 0.1f) {
				for (float z = min.z + forestObject.Radius / 2f; z <= max.z - forestObject.Radius / 2f; z += 0.1f) {
					Vector3 position = new Vector3(x, 0f, z);
					if (IsPossiblePosition(forestObject, position)) {
						possiblePositions.Add(position);
					}
				}
			}

			if (possiblePositions.Count == 0) {
				Debug.LogError($"No possible position found for {forestObject.name} at {i}");
				DestroyImmediate(forestObject.gameObject);
				return;
			}

			forestObject.transform.localPosition = possiblePositions[Random.Range(0, possiblePositions.Count)];
			instantiatedForestObjects.Add(forestObject);
		}
	}

	public void ClearForest() {
		instantiatedForestObjects.ForEach(item => DestroyImmediate(item.gameObject));
		instantiatedForestObjects.Clear();
	}

	private bool IsPossiblePosition(ForestObject forestObject, Vector3 position) {
		foreach (ForestObject obj in instantiatedForestObjects) {
			float distance = Vector3.Distance(position, obj.transform.localPosition);
			float minDistance = forestObject.Radius + obj.Radius;
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
		if (GUILayout.Button("Clear Forest")) {
			((ForestCreator)target).ClearForest();
		}

	}
}
#endif
