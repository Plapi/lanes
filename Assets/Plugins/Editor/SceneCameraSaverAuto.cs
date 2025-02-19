using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class SceneCameraSaverAuto {
	
	private static Vector3 lastPosition;
	private static Quaternion lastRotation;

	static SceneCameraSaverAuto() {
		// Hook into the Scene GUI to check for changes.
		SceneView.duringSceneGui += OnSceneGUI;
		// When a scene is opened, load its stored camera position.
		EditorSceneManager.sceneOpened += OnSceneOpened;
	}

	private static void OnSceneGUI(SceneView sceneView) {
		if (sceneView.camera == null) {
			return;
		}
		
		// If the camera's transform changed, save it.
		if (lastPosition != sceneView.camera.transform.position ||
		    lastRotation != sceneView.camera.transform.rotation) {
			lastPosition = sceneView.camera.transform.position;
			lastRotation = sceneView.camera.transform.rotation;

			// Use the current scene's name as a key.
			string sceneName = SceneManager.GetActiveScene().name;
			EditorPrefs.SetString("SceneCamPos_" + sceneName, JsonUtility.ToJson(lastPosition));
			EditorPrefs.SetString("SceneCamRot_" + sceneName, JsonUtility.ToJson(lastRotation));
		}
	}

	private static void OnSceneOpened(Scene scene, OpenSceneMode mode) {
		// When a scene is opened, try to restore the stored camera position.
		if (SceneView.lastActiveSceneView == null) {
			return;
		}
		
		string sceneName = scene.name;
		if (EditorPrefs.HasKey("SceneCamPos_" + sceneName) && EditorPrefs.HasKey("SceneCamRot_" + sceneName)) {
			Vector3 pos = JsonUtility.FromJson<Vector3>(EditorPrefs.GetString("SceneCamPos_" + sceneName));
			Quaternion rot = JsonUtility.FromJson<Quaternion>(EditorPrefs.GetString("SceneCamRot_" + sceneName));
			SceneView.lastActiveSceneView.pivot = pos;
			SceneView.lastActiveSceneView.rotation = rot;
			SceneView.lastActiveSceneView.Repaint();
		}
	}
}