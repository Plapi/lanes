using System;
using System.IO;
using UnityEditor;
using UnityEngine;
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
		
		if (GUILayout.Button("Test")) {
			
		}

		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndVertical();
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
