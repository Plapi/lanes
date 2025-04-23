using UnityEngine;

public class DebugFPS : MonoBehaviour {

	private const float FPS_REFRESH_INTERVAL = 1f;

	private GUIStyle guiStyle;
	private GUIStyle guiStyle1;
	private int fontSize;
	
	private float timer;
	private int fps;
	private float frameCounter;

	private void Awake() {
		fontSize = Screen.height * 3 / 100;
		guiStyle = new GUIStyle {
			alignment = TextAnchor.UpperLeft,
			fontSize = fontSize,
			fontStyle = FontStyle.Bold,
			normal = new GUIStyleState { textColor = Color.black }
		};
		guiStyle1 = new GUIStyle {
			alignment = TextAnchor.UpperLeft,
			fontSize = fontSize,
			fontStyle = FontStyle.Bold
		};
	}

	private void Update() {
		if (Time.unscaledDeltaTime <= 4 * Time.smoothDeltaTime) {
			timer += Time.unscaledDeltaTime;
			frameCounter++;
		}
		if (timer >= FPS_REFRESH_INTERVAL) {
			fps = Mathf.RoundToInt(frameCounter / timer);
			frameCounter = 0;
			timer = 0;
		}
	}

	private void OnGUI() {
		Rect rect = new Rect(0.05f * Screen.width, 0.02f * Screen.height, Screen.width, fontSize);
		GUI.Label(rect, fps.ToString(), guiStyle);
		rect.x -= 0.07f * fontSize;
		rect.y -= 0.07f * fontSize;
		guiStyle1.normal.textColor = fps < 15 ? Color.red : Color.yellow;
		GUI.Label(rect, fps.ToString(), guiStyle1);
	}
}