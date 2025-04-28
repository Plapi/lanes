using System;
using UnityEngine;
using UnityEngine.UI;
using frame8.Logic.Misc.Other.Extensions;
using Com.ForbiddenByte.OSA.Core;
using Com.ForbiddenByte.OSA.CustomParams;
using Com.ForbiddenByte.OSA.DataHelpers;
using TMPro;

public class UIParkingList : OSA<BaseParamsWithPrefab, ParkingListItemViewsHolder> {
	
	private SimpleDataHelper<ParkingSlotData> data;
	private Action<ParkingListItemViewsHolder> onClose;
	private Action<ParkingSlotData> onBuyTaxi;
	
	public void Init(ParkingSlotData[] parkingSlots, Action<ParkingSlotData> onBuyTaxi) {
		base.Start();
		data = new SimpleDataHelper<ParkingSlotData>(this);
		for (int i = 0; i < parkingSlots.Length; i++) {
			data.List.Add(parkingSlots[i]);
		}
		this.onBuyTaxi = onBuyTaxi;
		_Params.ItemPrefab.gameObject.SetActive(false);
		data.NotifyListChangedExternally();

		if (TryGetFirstEmptySlot(out int index) && index > 3) {
			ScrollTo(index);	
		}
	}

	private bool TryGetFirstEmptySlot(out int index) {
		for (int i = 0; i < data.List.Count; i++) {
			if (data.List[i].slotUnlocked && !data.List[i].taxiPurchased) {
				index = i;
				return true;
			}
		}
		index = -1;
		return false;
	}
	
	protected override ParkingListItemViewsHolder CreateViewsHolder(int itemIndex) {
		ParkingListItemViewsHolder instance = new ParkingListItemViewsHolder();
		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
		return instance;
	}

	protected override void UpdateViewsHolder(ParkingListItemViewsHolder newOrRecycled) {
		newOrRecycled.Init(data[newOrRecycled.ItemIndex], newOrRecycled.ItemIndex, onBuyTaxi);
	}
}

public class ParkingListItemViewsHolder : BaseItemViewsHolder {

	private ParkingSlotData parkingSlotData;
	private int index;
	private Action<ParkingSlotData> onBuy;

	private CanvasGroup canvasGroup;
	private Image emptySlotImage;
	private Image taxiSlotImage;
	private TextMeshProUGUI titleText;
	
	private Button buyButton;
	private CanvasGroup buyButtonCanvasGroup;
	private TextMeshProUGUI buyButtonText;
	
	private Image lockImage;

	public override void CollectViews() {
		base.CollectViews();
		root.GetComponentAtPath("CanvasGroup", out canvasGroup);
		root.GetComponentAtPath("CanvasGroup/EmptySlot", out emptySlotImage);
		root.GetComponentAtPath("CanvasGroup/TaxiSlot", out taxiSlotImage);
		root.GetComponentAtPath("CanvasGroup/Title", out titleText);
		root.GetComponentAtPath("Lock", out lockImage);
		root.GetComponentAtPath("BuyButton", out buyButton);
		root.GetComponentAtPath("BuyButton/CanvasGroup", out buyButtonCanvasGroup);
		root.GetComponentAtPath("BuyButton/CanvasGroup/Group/Text", out buyButtonText);
	}
	
	public void Init(ParkingSlotData parkingSlotData, int index, Action<ParkingSlotData> onBuy) {
		this.parkingSlotData = parkingSlotData;
		this.index = index;
		this.onBuy = onBuy;
		
		titleText.text = $"Slot {index + 1}";
		canvasGroup.alpha = parkingSlotData.slotUnlocked ? 1f : 0.5f;
		lockImage.gameObject.SetActive(!parkingSlotData.slotUnlocked);
		
		emptySlotImage.gameObject.SetActive(!parkingSlotData.taxiPurchased);
		taxiSlotImage.gameObject.SetActive(parkingSlotData.taxiPurchased);
		
		buyButton.gameObject.SetActive(parkingSlotData.slotUnlocked && !parkingSlotData.taxiPurchased);

		if (buyButton.gameObject.activeSelf) {
			UpdateBuyButton();
			GameController.Instance.OnCoinsUpdate += UpdateBuyButton;
		}
	}

	private void UpdateBuyButton() {
		int cost = Settings.Instance.company.parkingRoom.taxiCost;
		buyButtonText.text = cost.ToString("N0");
		GameController.Instance.EndOfFrame(() => {
			HorizontalLayoutGroup horizontalLayoutGroup = buyButtonText.transform.parent.GetComponent<HorizontalLayoutGroup>();
			horizontalLayoutGroup.enabled = false;
			horizontalLayoutGroup.enabled = true;
		});
		int coins = PlayerPrefsManager.UserData.coins;
		bool canBuy = coins >= cost;
		buyButton.interactable = canBuy;
		buyButton.onClick.RemoveAllListeners();
		buyButton.onClick.AddListener(() => {
			onBuy?.Invoke(parkingSlotData);
			Init(parkingSlotData, index, null);
		});
		buyButtonCanvasGroup.alpha = canBuy ? 1f : 0.5f;
	}

	public override void OnBeforeRecycleOrDisable(int newItemIndex) {
		base.OnBeforeRecycleOrDisable(newItemIndex);
		GameController.Instance.OnCoinsUpdate -= UpdateBuyButton;
	}
}