#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class RoomLevelCreator : MonoBehaviour {
    
	[SerializeField] private Object objFolder;

	[ContextMenu("Create")]
	private void Create() {
		
		if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(objFolder, out string guid, out long _)) {
			Debug.LogError($"Couldn't find asset GUID: {guid}");
			return;
		}
		string folderPath = AssetDatabase.GUIDToAssetPath(guid);

		MeshCombiner meshCombiner = gameObject.AddComponent<MeshCombiner>();
		
		Transform[] levels = new Transform[transform.childCount];
		for (int i = 0; i < levels.Length; i++) {
			levels[i] = transform.GetChild(i);
		}

		for (int i = 0; i < levels.Length; i++) {
			levels[i].parent = null;
		}

		for (int i = 0; i < levels.Length; i++) {
			levels[i].parent = transform;
			string levelName = $"{name}{levels[i].name[^1]}";
			guid = AssetDatabase.CreateFolder(folderPath, levelName);
			meshCombiner.SetOutput(AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid)), levelName);
			meshCombiner.Combine(transform);
		}
		
		DestroyImmediate(meshCombiner);
	}
	
}
#endif