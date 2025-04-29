using System;
using System.IO;
using UnityEngine;

public class ScreenshotTaker : MonoBehaviour {
	
	[SerializeField] private Camera mainCamera;

	private void Update() {
		if (Input.GetKeyDown(KeyCode.T)) {
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "pic.png");
			TakeScreenshot(path);
			System.Diagnostics.Process.Start(path);
		}
	}

	private void TakeScreenshot(string path) {
		int width = Screen.width;
		int height = Screen.height;

		RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
		mainCamera.targetTexture = rt;

		mainCamera.clearFlags = CameraClearFlags.SolidColor;
		mainCamera.backgroundColor = new Color(0, 0, 0, 0);

		mainCamera.Render();

		RenderTexture.active = rt;
		Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
		texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
		texture.Apply();

		File.WriteAllBytes(path, texture.EncodeToPNG());

		mainCamera.targetTexture = null;
		RenderTexture.active = null;
		Destroy(rt);
		Destroy(texture);
	}
}