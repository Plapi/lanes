using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class UIGaragePanel : UIPanel<UIGaragePanel.Data> {

	[SerializeField] private RectTransform topContainer;
	[SerializeField] private RectTransform bottomContainer;
	[SerializeField] private CanvasGroup centerContainer;
	
	[Space]
	[SerializeField] private TextMeshProUGUI coinsText;
	[SerializeField] private Button leftButton;
	[SerializeField] private Button rightButton;

	[Space]
	[SerializeField] private GameObject lockObj;
	[SerializeField] private UIChangeColor changeColor;
	
	[Space]
	[SerializeField] private Button goButton;
	[SerializeField] private Button buyButton;
	[SerializeField] private TextMeshProUGUI buyPriceText;

	protected override void OnInit() {
		leftButton.onClick.AddListener(data.onLeft);
		rightButton.onClick.AddListener(data.onRight);
		goButton.onClick.AddListener(data.onGo);
		buyButton.onClick.AddListener(data.onBuy);
		UpdateCoins(data.coins);
	}

	public void SetLeftRightButtonInteractable(bool leftInteractable, bool rightInteractable) {
		leftButton.interactable = leftInteractable;
		rightButton.interactable = rightInteractable;
	}

	public void UpdateCoins(int coins) {
		coinsText.text = coins.ToString("N0");
		this.EndOfFrame(() => {
			HorizontalLayoutGroup horizontalLayoutGroup = coinsText.transform.parent.GetComponent<HorizontalLayoutGroup>();
			horizontalLayoutGroup.enabled = false;
			horizontalLayoutGroup.enabled = true;
		});
	}

	public void UpdateBottom(int price) {
		bool showBuy = price > 0;
		goButton.gameObject.SetActive(!showBuy);
		buyButton.gameObject.SetActive(showBuy);
		lockObj.SetActive(showBuy);
		if (showBuy) {
			buyPriceText.text = price.ToString("N0");
			this.EndOfFrame(() => {
				HorizontalLayoutGroup horizontalLayoutGroup = buyPriceText.transform.parent.GetComponent<HorizontalLayoutGroup>();
				horizontalLayoutGroup.enabled = false;
				horizontalLayoutGroup.enabled = true;
			});
		}
	}

	public void InitChangeColor(Color[] colors, int selection, Action<int> onSelect) {
		changeColor.gameObject.SetActive(true);
		changeColor.Init(colors, selection, onSelect);
	}

	public void HideChangeColor() {
		changeColor.gameObject.SetActive(false);
	}

	protected override void CloseAnim(bool anim, Action onComplete) {
		if (anim) {
			topContainer.DOAnchorPosY(150f, UIController.defaultTime).SetEase(Ease.InQuad).OnComplete(() => {
				gameObject.SetActive(false);
				topContainer.SetAnchorPosY(0f);
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
			
			bottomContainer.DOAnchorPosY(-250f, UIController.defaultTime).SetEase(Ease.InQuad).OnComplete(() => {
				bottomContainer.SetAnchorPosY(0f);
			});
			
			centerContainer.DOFade(0f, UIController.defaultTime).OnComplete(() => {
				centerContainer.alpha = 1f;
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
		public UnityAction onBuy;
		public int coins;
	}
}
