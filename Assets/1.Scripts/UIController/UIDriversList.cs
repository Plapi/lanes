using System;
using UnityEngine;
using UnityEngine.UI;
using frame8.Logic.Misc.Other.Extensions;
using Com.ForbiddenByte.OSA.CustomAdapters.GridView;
using Com.ForbiddenByte.OSA.DataHelpers;
using TMPro;

public class UIDriversList : GridAdapter<GridParams, UIDriverItem> {

	private SimpleDataHelper<DriverData> data;
	private Action<DriverData> onHire;
	private Action<DriverData> onFire;
	private Action<DriverData> onSelect;

	public void Init(DriverData[] drivers, Action<DriverData> onHire, Action<DriverData> onFire, Action<DriverData> onSelect) {
		this.onHire = onHire;
		this.onFire = onFire;
		this.onSelect = onSelect;
		
		base.Start();
		data = new SimpleDataHelper<DriverData>(this);
		for (int i = 0; i < drivers.Length; i++) {
			data.List.Add(drivers[i]);
		}
		_Params.Grid.CellPrefab.gameObject.SetActive(false);
		data.NotifyListChangedExternally();
		ScrollTo(GetLastHiredDriver(drivers));
	}

	protected override void UpdateCellViewsHolder(UIDriverItem newOrRecycled) {
		newOrRecycled.Init(data[newOrRecycled.ItemIndex], onHire, onFire, onSelect);
	}
	
	private static int GetLastHiredDriver(DriverData[] drivers) {
		for (int i = drivers.Length - 1; i > 0; i--) {
			if (drivers[i].hired) {
				return i;
			}
		}
		return 0;
	}
}

public class UIDriverItem : CellViewsHolder {

	private DriverData driverData;
	private Action<DriverData> onHire;
	private Action<DriverData> onFire;
	private Action<DriverData> onSelect;
	
	private TextMeshProUGUI nameText;
	private TextMeshProUGUI incomeText;
	private HorizontalLayoutGroup starsGroup;

	private Image image;
	
	private Button hireButton;
	private CanvasGroup hireButtonCanvasGroup;
	private TextMeshProUGUI hireButtonText;
	
	private Button fireButton;
	private TextMeshProUGUI fireButtonText;
	
	private Button selectButton;
	private TextMeshProUGUI selectButtonText;
	
	public override void CollectViews() {
		base.CollectViews();
		views.GetComponentAtPath("Top/Top/Name", out nameText);
		views.GetComponentAtPath("Top/Top/IncomeText", out incomeText);
		views.GetComponentAtPath("Top/Stars", out starsGroup);
		views.GetComponentAtPath("Image", out image);
		views.GetComponentAtPath("HireButton", out hireButton);
		views.GetComponentAtPath("HireButton/CanvasGroup", out hireButtonCanvasGroup);
		views.GetComponentAtPath("HireButton/CanvasGroup/Group/Text", out hireButtonText);
		views.GetComponentAtPath("FireButton", out fireButton);
		views.GetComponentAtPath("FireButton/Group/Text", out fireButtonText);
		views.GetComponentAtPath("SelectButton", out selectButton);
		views.GetComponentAtPath("SelectButton/Group/Text", out selectButtonText);
	}

	public void Init(DriverData data, Action<DriverData> onHire, Action<DriverData> onFire, Action<DriverData> onSelect = null) {
		driverData = data;
		this.onHire = onHire;
		this.onFire = onFire;
		this.onSelect = onSelect;
		
		nameText.text = driverData.design.name;
		incomeText.text = $"+{Utils.FormatInt(driverData.design.income)}";
		GameController.Instance.EndOfFrame(() => {
			HorizontalLayoutGroup horizontalLayoutGroup = incomeText.transform.parent.GetComponent<HorizontalLayoutGroup>();
			horizontalLayoutGroup.enabled = false;
			horizontalLayoutGroup.enabled = true;
		});
		
		for (int i = 0; i < 5; i++) {
			starsGroup.transform.GetChild(i).GetChild(0).gameObject.SetActive(driverData.design.stars > i);
		}
		image.sprite = Resources.Load<Sprite>(driverData.design.spritePath);
		hireButton.gameObject.SetActive(!driverData.hired);
		fireButton.gameObject.SetActive(driverData.hired && onSelect == null);
		selectButton.gameObject.SetActive(driverData.hired && onSelect != null);
		
		if (!driverData.hired) {
			UpdateHireButton();
			GameController.Instance.OnCoinsUpdate += UpdateHireButton;
		} else {
			if (onSelect != null) {
				selectButton.onClick.RemoveAllListeners();
				selectButton.onClick.AddListener(() => onSelect(driverData));
				selectButtonText.text = PlayerPrefsManager.UserData.TryGetParkingSlotIndex(driverData, out int parkingSlotIndex) ? 
					$"Taxi {parkingSlotIndex + 1}" : "<color=#FF6000>Empty</color>";
				GameController.Instance.EndOfFrame(() => {
					HorizontalLayoutGroup horizontalLayoutGroup = selectButtonText.transform.parent.GetComponent<HorizontalLayoutGroup>();
					horizontalLayoutGroup.enabled = false;
					horizontalLayoutGroup.enabled = true;
				});
			} else {
				fireButtonText.text = $"+{Utils.FormatInt(driverData.design.fireCost)}";
				GameController.Instance.EndOfFrame(() => {
					HorizontalLayoutGroup horizontalLayoutGroup = fireButtonText.transform.parent.GetComponent<HorizontalLayoutGroup>();
					horizontalLayoutGroup.enabled = false;
					horizontalLayoutGroup.enabled = true;
				});
				fireButton.onClick.RemoveAllListeners();
				fireButton.onClick.AddListener(() => {
					onFire?.Invoke(driverData);
					Init(driverData, onHire, onFire);
				});
			}
		}
	}
	
	private void UpdateHireButton() {
		hireButtonText.text = Utils.FormatInt(driverData.design.hireCost);
		GameController.Instance.EndOfFrame(() => {
			HorizontalLayoutGroup horizontalLayoutGroup = hireButtonText.transform.parent.GetComponent<HorizontalLayoutGroup>();
			horizontalLayoutGroup.enabled = false;
			horizontalLayoutGroup.enabled = true;
		});
		int coins = PlayerPrefsManager.UserData.coins;
		bool canBuy = coins >= driverData.design.hireCost;
		hireButton.interactable = canBuy;
		hireButton.onClick.RemoveAllListeners();
		hireButton.onClick.AddListener(() => {
			onHire?.Invoke(driverData);
			Init(driverData, onHire, onFire, onSelect);
		});
		hireButtonCanvasGroup.alpha = canBuy ? 1f : 0.5f;
	}
	
	public override void OnBeforeRecycleOrDisable(int newItemIndex) {
		base.OnBeforeRecycleOrDisable(newItemIndex);
		GameController.Instance.OnCoinsUpdate -= UpdateHireButton;
	}
}