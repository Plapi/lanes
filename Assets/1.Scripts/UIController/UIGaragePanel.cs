using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class UIGaragePanel : UIPanel<UIGaragePanel.Data> {

	[Space] 
	[SerializeField] private RectTransform coinsContainer;
	[SerializeField] private TextMeshProUGUI coinsText;
	
	[Space]
	[SerializeField] private Button leftButton;
	[SerializeField] private Button rightButton;
	
	[Space]
	[SerializeField] private Button goButton;

	protected override void OnInit() {
		leftButton.onClick.AddListener(data.onLeft);
		rightButton.onClick.AddListener(data.onRight);
		goButton.onClick.AddListener(data.onGo);
		UpdateCoins(data.coins);
	}

	public void SetLeftRightButtonInteractable(bool leftInteractable, bool rightInteractable) {
		leftButton.interactable = leftInteractable;
		rightButton.interactable = rightInteractable;
	}

	public void UpdateCoins(int coins) {
		coinsText.text = coins.ToString("N0");
		this.EndOfFrame(() => {
			HorizontalLayoutGroup horizontalLayoutGroup = coinsContainer.GetComponent<HorizontalLayoutGroup>();
			horizontalLayoutGroup.enabled = false;
			horizontalLayoutGroup.enabled = true;
		});
	}

	protected override void CloseAnim(bool anim, Action onComplete) {
		if (anim) {
			float initCoinsContainerY = coinsContainer.anchoredPosition.y;
			coinsContainer.DOAnchorPosY(90f, UIController.defaultTime).SetEase(Ease.InQuad).OnComplete(() => {
				gameObject.SetActive(false);
				coinsContainer.SetAnchorPosY(initCoinsContainerY);
				onComplete?.Invoke();
			});
			
			RectTransform leftButtonRectTransform = leftButton.GetComponent<RectTransform>();
			float initLeftX = leftButtonRectTransform.anchoredPosition.x;
			leftButtonRectTransform.DOAnchorPosX(-70f, UIController.defaultTime).SetEase(Ease.InQuad).OnComplete(() => {
				leftButtonRectTransform.SetAnchorPosX(initLeftX);
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
		public int coins;
	}
}
