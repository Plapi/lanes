using UnityEngine;

public class InputManager : MonoBehaviour {

	public float VerticalInput { get; private set; }
	public float HorizontalInput { get; private set; }

	private LayerMask layerMask;
	
	private void Awake() {
		VerticalInput = 0.5f;
		HorizontalInput = 0.5f;
	}

	private void Update() {
		if (Input.GetMouseButton(0)) {
			float y = Input.mousePosition.y / Screen.height;
			VerticalInput = Mathf.InverseLerp(0f, 0.2f, y);
			HorizontalInput = Input.mousePosition.x / Screen.width;
		} else {
			HorizontalInput = 0.5f;
		}
	}
}
