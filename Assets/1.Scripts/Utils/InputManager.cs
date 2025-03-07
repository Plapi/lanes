using System;
using ArcadeVP;
using UnityEngine;

[ExecuteInEditMode]
public class InputManager : MonoBehaviour {

	[SerializeField] private float verticalMinY;
	[SerializeField] private float verticalMaxY;
	[SerializeField] private bool debug;

	public float VerticalInput { get; private set; }
	public float HorizontalInput { get; private set; }

	private LayerMask layerMask;

	private void Awake() {
		ResetValues();
	}

	public void ResetValues() {
		VerticalInput = 0.5f;
		HorizontalInput = 0.5f;
	}

	private void Update() {
		if (!Application.isPlaying) {
			return;
		}
		if (Input.GetMouseButton(0)) {
			float y = Input.mousePosition.y / Screen.height;
			if (y > 0.7f) {
				return;
			}
			VerticalInput = Mathf.InverseLerp(verticalMinY, verticalMaxY, y);
			HorizontalInput = Input.mousePosition.x / Screen.width;
		} else {
			HorizontalInput = 0.5f;
		}
	}

	[NonSerialized] private Texture2D texture;
	[NonSerialized] private GUIStyle boxStyle;
	[NonSerialized] private GUIStyle labelStyle;
	[NonSerialized] private ArcadeVehicleController avc;

	private void OnGUI() {

		if (!Application.isPlaying || !debug) {
			return;
		}
		
		if (avc == null) {
			if (GameController.Instance.GetUserCar() == null) {
				return;
			}
			avc = GameController.Instance.GetUserCar().GetComponent<ArcadeVehicleController>();
		}
		
		if (texture == null) {
			texture = new Texture2D(1, 1);
			texture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.5f));
			texture.Apply();
			texture.wrapMode = TextureWrapMode.Repeat;
		}
		boxStyle ??= new GUIStyle {
			normal = {
				background = texture,
				textColor = Color.white
			}
		};
		labelStyle ??= new GUIStyle {
			fontSize = Mathf.RoundToInt(Screen.width * 0.05f)
		};
		GUI.Box(new Rect(0f, Screen.height - Screen.height * verticalMaxY, Screen.width,
			Screen.height * verticalMaxY - Screen.height * verticalMinY), GUIContent.none, boxStyle);
		
		string text = VerticalInput.ToString("F2");
		text += $"\n{avc.AccelerationInput:F2}";
		text += $"\n{avc.BrakeInput:F2}";
		text += $"\n{avc.CurrentSpeed:F2}";
		text += $"\n{avc.MaxSpeed:F2}";
		GUI.Label(new Rect(0f, Screen.height - Screen.height * verticalMaxY, Screen.width, Screen.height * 0.1f),
			text, labelStyle);
	}
}