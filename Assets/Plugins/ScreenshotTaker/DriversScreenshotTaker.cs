using System;
using System.IO;
using UnityEngine;
using UnityEditor;

public class DriversScreenshotTaker : MonoBehaviour {
    
	[SerializeField] private UnityEngine.Object objFolder;
	[SerializeField] private GameObject[] characters;
	[SerializeField] private new Camera camera;

	private void Update() {
		if (Input.GetKeyDown(KeyCode.T)) {

			int driverIndex = 0;
			string s = "";
			
			for (int i = 0; i < characters.Length; i++) {
				for (int j = 0; j < characters[i].transform.childCount - 1; j++) {
					Transform child = characters[i].transform.GetChild(j);
					child.gameObject.SetActive(true);
					
					string path = $"{Application.dataPath.Replace("Assets", string.Empty)}{AssetDatabase.GetAssetPath(objFolder)}/Driver{driverIndex}.png";
					File.WriteAllBytes(path, TakeScreenshot());
					
					driverIndex++;
					child.gameObject.SetActive(false);

					s += child.name.Replace("Character_", string.Empty) + "\n";
				}
			}
			
			AssetDatabase.Refresh();
			Debug.LogError(s);
		}
	}
	
	private byte[] TakeScreenshot() {
		int width = Screen.width;
		int height = Screen.height;

		RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
		camera.targetTexture = rt;

		camera.clearFlags = CameraClearFlags.SolidColor;
		camera.backgroundColor = new Color(0, 0, 0, 0);

		camera.Render();

		RenderTexture.active = rt;
		Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
		texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
		texture.Apply();

		byte[] bytes = texture.EncodeToPNG();
		// File.WriteAllBytes(path, texture.EncodeToPNG());

		camera.targetTexture = null;
		RenderTexture.active = null;
		Destroy(rt);
		Destroy(texture);

		return bytes;
	}
}
