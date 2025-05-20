using UnityEngine;
using UnityEngine.UI;

public class UIDrivingInput : UIObject {

	[SerializeField] private Image circle;
	[SerializeField] private Image background;

	private float backgroundTop;
	private float backgroundBottom;
	private float backgroundLeft;
	private float backgroundRight;
	
	private bool isActive;
	
	private void Start() {
		Vector2 backgroundCenter = new Vector2(0f, -RectTransform.rect.size.y / 2f + background.rectTransform.anchoredPosition.y + background.rectTransform.rect.size.y / 2f);
		backgroundTop = backgroundCenter.y + background.rectTransform.rect.size.y / 2f;
		backgroundBottom = backgroundCenter.y - background.rectTransform.rect.size.y / 2f;
		backgroundLeft = backgroundCenter.x - background.rectTransform.rect.size.x / 2f;
		backgroundRight = backgroundCenter.x + background.rectTransform.rect.size.x / 2f;
	}
	
	private void Update() {
		if (Input.GetMouseButtonDown(0)) {
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, Input.mousePosition, null, out Vector2 localPoint)) {
				isActive = localPoint.x >= backgroundLeft && localPoint.x <= backgroundRight && localPoint.y >= backgroundBottom && localPoint.y <= backgroundTop;
			}
		} else if (Input.GetMouseButton(0)) {
			if (isActive && RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, Input.mousePosition, null, out Vector2 localPoint)) {
				localPoint.y = Mathf.Clamp(localPoint.y, backgroundBottom, backgroundTop);
				localPoint.x = Mathf.Clamp(localPoint.x, backgroundLeft, backgroundRight);
				circle.rectTransform.anchoredPosition = localPoint;
				circle.SetAlpha(0.6f);
				background.SetAlpha(0.3f);
			}
		} else {
			isActive = false;
			circle.rectTransform.SetAnchorPosX(0f);
			circle.SetAlpha(0.3f);
			background.SetAlpha(0.1f);
		}
	}

}
