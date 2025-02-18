using System;
using UnityEngine;

public class InputManager : MonoBehaviour {

	public Action<float> OnHorizontalInput;

	public float VerticalInput { get; private set; }

	private bool swiped;
	
	private void Awake() {
		VerticalInput = 0.5f;
	}

	private void Update() {
		if (Input.GetMouseButton(0)) {
			float y = Input.mousePosition.y / Screen.height;
			VerticalInput = y < 0.08f ? -1f : Mathf.InverseLerp(0.1f, 0.3f, y);
			
			float x = Input.mousePosition.x / Screen.width - 0.5f;
			if (!swiped) {
				if (x < -0.1f) {
					OnHorizontalInput?.Invoke(-1f);
				} else if (x > 0.1f) {
					OnHorizontalInput?.Invoke(1f);
				}	
			}
			swiped = Mathf.Abs(x) > 0.1f;
		} else {
			swiped = false;
		}
	}
}
