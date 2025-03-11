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
	[SerializeField] private Slider speedSlider;
	[SerializeField] private Slider healthSlider;
	
	[Space]
	[SerializeField] private TextMeshProUGUI coinsText;
	[SerializeField] private Button coinButton;

	[Space]
	[SerializeField] private Button settingsButton;
	[SerializeField] private Button leftButton;
	[SerializeField] private Button rightButton;

	[Space]
	[SerializeField] private GameObject lockObj;
	[SerializeField] private UIChangeColor changeColor;
	
	[Space]
	[SerializeField] private Button goButton;
	[SerializeField] private Button buyButton;
	[SerializeField] private TextMeshProUGUI buyPriceText;
	
	[Space]
	[SerializeField] private UICoinsAnim coinsAnim;

	protected override void OnInit() {
		settingsButton.onClick.AddListener(data.onSettings);
		leftButton.onClick.AddListener(data.onLeft);
		rightButton.onClick.AddListener(data.onRight);
		goButton.onClick.AddListener(data.onGo);
		buyButton.onClick.AddListener(data.onBuy);
		coinButton.onClick.AddListener(data.onCoin);
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

	public void PlayCoinsAnim(int from, int to, int count = 10) {
		int coins = from;
		int add = (to - from) / count;
		coinsAnim.OnCoinReach = () => {
			coins += add;
			UpdateCoins(coins);
			Transform coinsContainer = coinsText.transform.parent;
			coinsContainer.DOKill();
			coinsContainer.transform.localScale = Vector3.one;
			coinsContainer.DOPunchScale(Vector3.one * 0.2f, UIController.defaultTime);
		};
		coinsAnim.Play(10, () => {
			UpdateCoins(to);
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

	public void UpdateSliders(float speed, float health, bool instant) {
		speedSlider.DOKill();
		healthSlider.DOKill();
		if (instant) {
			speedSlider.value = speed;
			healthSlider.value = health;
		} else {
			speedSlider.DOValue(speed, UIController.defaultTime).SetEase(Ease.OutQuad);
			healthSlider.DOValue(health, UIController.defaultTime).SetEase(Ease.OutQuad);
		}
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
		public UnityAction onSettings;
		public UnityAction onLeft;
		public UnityAction onRight;
		public UnityAction onGo;
		public UnityAction onBuy;
		public UnityAction onCoin;
		public int coins;
	}
}
