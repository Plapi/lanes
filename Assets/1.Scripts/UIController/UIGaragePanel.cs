using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIGaragePanel : UIPanel<UIGaragePanel.Data> {
    
	[Space]
	[SerializeField] private Button leftButton;
	[SerializeField] private Button rightButton;
	[SerializeField] private Button goButton;

	protected override void OnInit() {
		leftButton.onClick.AddListener(data.onLeft);
		rightButton.onClick.AddListener(data.onRight);
		goButton.onClick.AddListener(data.onGo);
	}

	public void SetLeftRightButtonInteractable(bool leftInteractable, bool rightInteractable) {
		leftButton.interactable = leftInteractable;
		rightButton.interactable = rightInteractable;
	}

	protected override void CloseAnim(bool anim, Action onComplete) {
		if (anim) {
			RectTransform leftButtonRectTransform = leftButton.GetComponent<RectTransform>();
			float initLeftX = leftButtonRectTransform.anchoredPosition.x;
			leftButtonRectTransform.DOAnchorPosX(-70f, UIController.defaultTime).SetEase(Ease.InQuad).OnComplete(() => {
				gameObject.SetActive(false);
				leftButtonRectTransform.SetAnchorPosX(initLeftX);
				onComplete?.Invoke();
			});
			RectTransform rightButtonRectTransform = rightButton.GetComponent<RectTransform>();
			float initRightX = rightButtonRectTransform.anchoredPosition.x;
			rightButtonRectTransform.DOAnchorPosX(70f, UIController.defaultTime).SetEase(Ease.InQuad).OnComplete(() => {
				rightButtonRectTransform.SetAnchorPosX(initRightX);
			});
			RectTransform goButtonRectTransform = goButton.GetComponent<RectTransform>();
			float initGoY = goButtonRectTransform.anchoredPosition.y;
			goButtonRectTransform.DOAnchorPosY(-150f, UIController.defaultTime).SetEase(Ease.InQuad).OnComplete(() => {
				goButtonRectTransform.SetAnchorPosY(initGoY);
			});
		} else {
			gameObject.SetActive(false);
			onComplete?.Invoke();
		}
	}

	public new class Data: UIPanelBase.Data {
		public UnityAction onLeft;
		public UnityAction onRight;
		public UnityAction onGo;
	}
}
