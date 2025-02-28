using UnityEngine;

public static class RectTransformExtensions {
	
	public static void SetAnchorPosX(this RectTransform rectTransform, float x) {
		rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);
	}
	
	public static void SetAnchorPosY(this RectTransform rectTransform, float y) {
		rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, y);
	}
}