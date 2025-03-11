using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using JsonFx.Json;

public class DebugWindow : EditorWindow {

	private static Vector2 s_scrollPos {
		get => new Vector2(0f, EditorPrefs.GetFloat("EDITOR_WINDOW_SCROLL_POS_Y", 0f));
		set => EditorPrefs.SetFloat("EDITOR_WINDOW_SCROLL_POS_Y", value.y);
	}
	
	[MenuItem("Window/Debug Window")]
	private static void Init() {
		((DebugWindow)GetWindow(typeof(DebugWindow))).Show();
	}

	private void OnGUI() {
		EditorGUILayout.BeginVertical();
		if (!Application.isPlaying) {
			SceneNavigator.Display();
		}

		s_scrollPos = EditorGUILayout.BeginScrollView(s_scrollPos);

		Time.timeScale = EditorGUILayout.Slider("Time Scale", Time.timeScale, 0f, 1f);

		if (GUILayout.Button("Collapse")) {
			EditorCollapseAll.CollapseFolders();
		}
		
		if (GUILayout.Button("Delete Player Prefs")) {
			PlayerPrefs.DeleteAll();
			PlayerPrefs.Save();
		}

		if (GUILayout.Button("Add Coins")) {
			PlayerPrefsManager.UserData.coins += 1000;
			FindAnyObjectByType<UIGaragePanel>().UpdateCoins(PlayerPrefsManager.UserData.coins);
		}

		if (GUILayout.Button("Set Buttons Sound")) {
			SetButtonsSound();
		}
		
		if (GUILayout.Button("Take Screenshot")) {
			EditorCoroutine.Start(TakeScreenshotIEnumerator());
		}

		if (GUILayout.Button("Test")) {
			
		}

		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndVertical();
	}

	private static void SetButtonsSound() {
		const string path = "Assets/Free UI Click Sound Effects Pack/AUDIO/Crispy/SFX_UI_Click_Organic_Crispy_Generic_Select_1.wav";
		AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);

		Button[] buttons = Resources.FindObjectsOfTypeAll<Button>();
		foreach (Button button in buttons) {
			if (!button.TryGetComponent(out UIButtonSound buttonSound)) {
				buttonSound = button.gameObject.AddComponent<UIButtonSound>();
			}
			buttonSound.AddButtonEvent(button, audioClip);
		}

	}
	
	[MenuItem("Editor/Take Screenshot %k")]
	private static void TakeScreenshot() {
		EditorCoroutine.Start(TakeScreenshotIEnumerator());
	}
	
	private static IEnumerator TakeScreenshotIEnumerator() {
		string screenCaptureName = "ScreenCapture " + DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss") + ".png";

		ScreenCapture.CaptureScreenshot(screenCaptureName);
		while (!File.Exists(Application.dataPath.Replace("Assets", screenCaptureName))) {
			yield return null;
		}

		string screenshotPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/" + screenCaptureName;

		File.WriteAllBytes(screenshotPath, File.ReadAllBytes(Application.dataPath.Replace("Assets", screenCaptureName)));
		File.Delete(Application.dataPath.Replace("Assets", screenCaptureName));

		System.Diagnostics.Process m_process = new System.Diagnostics.Process {
			StartInfo = new System.Diagnostics.ProcessStartInfo(screenshotPath)
		};

		m_process.Start();
	}
	
	[MenuItem("Editor/Play From Splash %g")]
	public static void PlayFromSplash() {
		if (Application.isPlaying) {
			EditorApplication.ExecuteMenuItem("Edit/Play");
			string lastScenePath = EditorPrefs.GetString("LANES_LAST_SCENE", "");
			if (!string.IsNullOrEmpty(lastScenePath)) {
				EditorCoroutine.Start(WaitToApplicationClose(() => {
					EditorSceneManager.OpenScene(lastScenePath);
					SelectLastGameObjectSelected();
				}));
				EditorPrefs.DeleteKey("LANES_LAST_SCENE");
			}
			return;
		}
		
		CheckSaveScene(() => {
			string[] assets = AssetDatabase.FindAssets("t:scene", new string[] { "Assets/0.Scenes" });
			for (int i = 0; i < assets.Length; i++) {
				SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(assets[i]));
				if (scene != null && scene.name == "Splash") {
					CheckSaveScene(() => {
						SaveLastGameObjectSelected(Selection.activeGameObject);
						EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(assets[i]));
						EditorApplication.ExecuteMenuItem("Edit/Play");
					});
					return;
				}
			}
			Debug.LogError("Scene Splash not found");
		});
	}
	
	[MenuItem("Editor/Reload Current Scene Or Prefab %t")]
	public static void ReloadCurrentSceneOrPrefab() {
		if (!Application.isPlaying) {
			PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
			if (prefabStage != null) {
				AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(prefabStage.assetPath));
			} else {
				CheckSaveScene(() => {
					SaveLastGameObjectSelected(Selection.activeGameObject);
					EditorSceneManager.OpenScene(SceneManager.GetActiveScene().path);
					SelectLastGameObjectSelected();
				});
			}
		}
	}
	
	private static IEnumerator WaitToApplicationClose(Action onComplete) {
		while (Application.isPlaying) {
			yield return null;
		}
		onComplete();
	}
	
	private static void SaveLastGameObjectSelected(GameObject _selectedGameObject) {
		EditorPrefs.SetString("LANES_LAST_GAMEOBJECT_SELECTED_NAME", _selectedGameObject != null ? _selectedGameObject.name : null);
	}
	
	private static void SelectLastGameObjectSelected() {
		Selection.activeGameObject = GameObject.Find(EditorPrefs.GetString("LANES_LAST_GAMEOBJECT_SELECTED_NAME"));
	}
	
	private static void CheckSaveScene(Action onComplete) {
		Scene scene = SceneManager.GetActiveScene();
		EditorPrefs.SetString("LANES_LAST_SCENE", scene.path);
		if (scene.isDirty) {
			if (EditorUtility.DisplayDialog("Save Scene", "Do you want to save " + scene.name + " before playing?", "Yes", "No")) {
				EditorSceneManager.SaveScene(scene, "", false);
				onComplete();
			} else {
				onComplete();
			}
		} else {
			onComplete();
		}
	}

	private void CropTexture(string inPath, string outPath, int startX, int startY) {
		Material mat = (Material)AssetDatabase.LoadAssetAtPath(inPath, typeof(Material));

		Texture2D texture = DuplicateTexture((Texture2D)mat.mainTexture);
		Texture2D cropTexture = new Texture2D(124, 124);

		for (int x = 0; x < cropTexture.width; x++) {
			for (int y = 0; y < cropTexture.height; y++) {
				cropTexture.SetPixel(x, y, texture.GetPixel(x + startX, y + startY));
			}
		}

		cropTexture.Apply();

		File.WriteAllBytes(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + outPath, cropTexture.EncodeToPNG());
	}

	Texture2D DuplicateTexture(Texture2D source) {
		byte[] pix = source.GetRawTextureData();
		Texture2D readableText = new Texture2D(source.width, source.height, source.format, false);
		readableText.LoadRawTextureData(pix);
		readableText.Apply();
		return readableText;
	}

	private static class SceneNavigator {

		private const string EDITOR_NAVIGATOR_SCENES = "CB_EDITOR_NAVIGATOR_SCENES";
		private const int MAX_NAVIGATOR_SCENES = 5;

		private static string[] navigatorScenes {
			get => JsonReader.Deserialize<string[]>(EditorPrefs.GetString(EDITOR_NAVIGATOR_SCENES, JsonWriter.Serialize(new string[] { })));
			set => EditorPrefs.SetString(EDITOR_NAVIGATOR_SCENES, JsonWriter.Serialize(value));
		}

		private static void TryToAddScene(string scene) {
			List<string> scenes = new List<string>(navigatorScenes);
			if (!scenes.Contains(scene)) {
				scenes.Insert(0, scene);
				while (scenes.Count > MAX_NAVIGATOR_SCENES) {
					scenes.RemoveAt(scenes.Count - 1);
				}
				navigatorScenes = scenes.ToArray();
			}
		}

		public static void Reset() {
			navigatorScenes = new string[0];
		}

		public static void Display() {
			Scene scene = SceneManager.GetActiveScene();
			if (string.IsNullOrEmpty(scene.path)) {
				return;
			}
			TryToAddScene(scene.path);

			List<string> scenes = new List<string>(navigatorScenes);
			string[] sceneNames = new string[scenes.Count];
			int selectedScene = 0;

			for (int i = 0; i < scenes.Count; i++) {
				SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenes[i]);
				if (sceneAsset != null) {
					sceneNames[i] = sceneAsset.name;
				}
				if (scene.path == scenes[i]) {
					selectedScene = i;
				}
			}

			int newSelectedScene = GUILayout.SelectionGrid(selectedScene, sceneNames, scenes.Count);
			if (newSelectedScene != selectedScene) {
				if (Event.current.button == 0) {
					CheckSaveScene(() => {
						EditorSceneManager.OpenScene(scenes[newSelectedScene]);
					});
				} else {
					scenes.RemoveAt(newSelectedScene);
					navigatorScenes = scenes.ToArray();
				}
			}
		}

		private static void CheckSaveScene(Action _onComplete) {
			Scene scene = SceneManager.GetActiveScene();
			if (scene.isDirty) {
				if (EditorUtility.DisplayDialog("Save Scene", "Do you want to save " + scene.name + "?", "Yes", "No")) {
					EditorSceneManager.SaveScene(scene, "", false);
					_onComplete();
				} else {
					_onComplete();
				}
			} else {
				_onComplete();
			}
		}
	}
}
