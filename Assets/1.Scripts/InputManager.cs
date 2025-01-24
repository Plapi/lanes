using System;
using UnityEngine;

public class InputManager : MonoBehaviour {

	public Action<float> OnHorizontalInput;

	public float VerticalInput { get; private set; }
	
	private void Update() {
		if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
			OnHorizontalInput?.Invoke(-1f);
		} else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
			OnHorizontalInput?.Invoke(1f);
		}
		
		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
			VerticalInput = 1f;
		} else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
			VerticalInput = -1f;
		} else {
			VerticalInput = 0f;
		}
	}
}
