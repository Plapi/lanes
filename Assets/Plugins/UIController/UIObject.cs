using UnityEngine;

public class UIObject : MonoBehaviour {
	private RectTransform rectTransform;
	protected RectTransform RectTransform {
		get {
			if (rectTransform == null) {
				rectTransform = GetComponent<RectTransform>();
			}
			return rectTransform;
		}
	}
}