using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine.Events;

public class UIShopPanel : UIPanel<UIShopPanel.Data> {

	[Space]
	[SerializeField] private Button removeAdsButton;
	[SerializeField] private GameObject purchasedObject;

	[Space]
	[SerializeField] private Button[] coinsButtons;
	
	protected override void OnInit() {
		UpdateRemoveAdsButton(PlayerPrefsManager.UserData.removeAdsPurchased);
	}

	private void Start() {
		removeAdsButton.onClick.AddListener(() => {
			PlayerPrefsManager.UserData.removeAdsPurchased = true;
			PlayerPrefsManager.SaveUserData();
			UpdateRemoveAdsButton(true);
		});
		coinsButtons[0].onClick.AddListener(() => data.addCoins(100000));
		coinsButtons[1].onClick.AddListener(() => data.addCoins(300000));
		coinsButtons[2].onClick.AddListener(() => data.addCoins(1000000));
		coinsButtons[3].onClick.AddListener(() => data.addCoins(2000000));
		coinsButtons[4].onClick.AddListener(() => data.addCoins(10000000));
	}

	private void UpdateRemoveAdsButton(bool removeAdsPurchased) {
		removeAdsButton.interactable = !removeAdsPurchased;
		purchasedObject.SetActive(removeAdsPurchased);
	}

	protected override void ShowAnim(Action onComplete) {
		gameObject.SetActive(true);
		RectTransform contentRect = content.GetComponent<RectTransform>();
		contentRect.SetAnchorPosY(-1170f);
		contentRect.DOAnchorPosY(-70f, UIController.defaultTime).SetEase(Ease.OutQuad).OnComplete(() => onComplete());
	}
	
	protected override void CloseAnim(bool anim, Action onComplete) {
		RectTransform contentRect = content.GetComponent<RectTransform>();
		contentRect.DOAnchorPosY(-1170f, UIController.defaultTime).SetEase(Ease.InQuad).OnComplete(() => gameObject.SetActive(false));
		onComplete();
	}
	
	public new class Data : UIPanelBase.Data {
		public UnityAction<int> addCoins;
	}
}
