using UnityEngine;

public class UIObject : MonoBehaviour {
	private RectTransform rectTransform;
	public RectTransform RectTransform {
		get {
			if (rectTransform == null) {
				rectTransform = GetComponent<RectTransform>();
			}
			return rectTransform;
		}
	}
}