using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class UIGaragePanel : UIPanel<UIGaragePanel.Data> {

	[Space]
	[SerializeField] private RectTransform topContainer;
	[SerializeField] private RectTransform bottomContainer;
	[SerializeField] private CanvasGroup centerContainer;
	
	[Space]
	[SerializeField] private Slider speedSlider;
	[SerializeField] private Slider healthSlider;

	[Space]
	[SerializeField] private Button closeButton;
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
	
	public RectTransform TopContainer => topContainer;

	protected override void OnInit() {
		closeButton.onClick.AddListener(data.onCloseButton);
		leftButton.onClick.AddListener(data.onLeft);
		rightButton.onClick.AddListener(data.onRight);
		goButton.onClick.AddListener(data.onGo);
		buyButton.onClick.AddListener(data.onBuy);
	}

	public void SetLeftRightButtonInteractable(bool leftInteractable, bool rightInteractable) {
		leftButton.interactable = leftInteractable;
		rightButton.interactable = rightInteractable;
	}

	public void PlayCoinsAnim(int from, int to, int count = 10) {
		UICoins coinsPanel = UIController.Instance.GetPanel<UIMainPanel>().CoinsPanel;
		int coins = from;
		int add = (to - from) / count;
		Vector3 scale = coinsPanel.transform.localScale;
		coinsAnim.OnCoinReach = () => {
			coins += add;
			coinsPanel.UpdateCoins(coins);
			coinsPanel.transform.DOKill();
			coinsPanel.transform.localScale = scale;
			coinsPanel.transform.DOPunchScale(scale * 0.2f, UIController.defaultTime);
		};
		coinsAnim.Play(10, () => {
			coinsPanel.UpdateCoins(to);
		});
	}

	public void UpdateBottom(int price) {
		bool showBuy = price > 0;
		goButton.gameObject.SetActive(!showBuy);
		buyButton.gameObject.SetActive(showBuy);
		lockObj.SetActive(showBuy);
		if (showBuy) {
			buyPriceText.text = Utils.FormatInt(price);
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
		public UnityAction onCloseButton;
		public UnityAction onLeft;
		public UnityAction onRight;
		public UnityAction onGo;
		public UnityAction onBuy;
	}
}
