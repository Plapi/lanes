using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using TMPro;
using DG.Tweening;

public class UIShopPanel : UIPanel<UIShopPanel.Data> {

	[Space]
	[SerializeField] private Button removeAdsButton;
	[SerializeField] private GameObject purchasedObject;

	[Space]
	[SerializeField] private Button[] coinsButtons;
	
	protected override void OnInit() {
		UpdateRemoveAdsButton(PlayerPrefsManager.UserData.removeAdsPurchased);
		UIController.Instance.ShowLoading();
		InAppPurchasingController.Instance.InitIfNeeded(InitUI);
	}

	private void InitUI(ProductCollection products) {
		UIController.Instance.HideLoading();
		
		if (products == null) {
			return;
		}
		
		InAppPurchaseProduct[] localProducts = Settings.Instance.inAppPurchaseProducts;
		GameObject[] productObjects = new GameObject[localProducts.Length];
		productObjects[0] = removeAdsButton.gameObject;
		for (int i = 0; i < coinsButtons.Length; i++) {
			productObjects[i + 1] = coinsButtons[i].gameObject;
		}
		
		for (int i = 0; i < productObjects.Length; i++) {
			Product product = products.WithID(localProducts[i].id);
			if (product != null) {
				TextMeshProUGUI costText = productObjects[i].transform.Find("CostText").GetComponent<TextMeshProUGUI>();
				costText.text = product.metadata.localizedPriceString;
			} else {
				Debug.LogError($"Product not found {localProducts[i].id}");
			}
		}
	}
	
	private void Start() {
		removeAdsButton.onClick.AddListener(() => {
			UIController.Instance.ShowLoading();
			InAppPurchasingController.Instance.Purchase(Settings.Instance.inAppPurchaseProducts[0], success => {
				if (success) {
					PlayerPrefsManager.UserData.removeAdsPurchased = true;
					PlayerPrefsManager.SaveUserData();
					UpdateRemoveAdsButton(true);		
				}
				UIController.Instance.HideLoading();
			});
		});
		for (int i = 0; i < coinsButtons.Length; i++) {
			int ii = i;
			coinsButtons[i].onClick.AddListener(() => {
				UIController.Instance.ShowLoading();
				InAppPurchasingController.Instance.Purchase(Settings.Instance.inAppPurchaseProducts[ii + 1], success => {
					if (success) {
						data.addCoins(Settings.Instance.inAppPurchaseProducts[ii + 1].value);
					}
					UIController.Instance.HideLoading();
				});
			});
		}
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
