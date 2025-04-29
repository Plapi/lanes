using UnityEngine;
using UnityEngine.UI;
using frame8.Logic.Misc.Other.Extensions;
using Com.ForbiddenByte.OSA.CustomAdapters.GridView;
using Com.ForbiddenByte.OSA.DataHelpers;
using TMPro;

public class UIDriversList : GridAdapter<GridParams, UIDriverItem> {

	private SimpleDataHelper<DriverDesignData> data;

	public void Init(DriverDesignData[] drivers) {
		base.Start();
		data = new SimpleDataHelper<DriverDesignData>(this);
		for (int i = 0; i < drivers.Length; i++) {
			data.List.Add(drivers[i]);
		}
		_Params.Grid.CellPrefab.gameObject.SetActive(false);
		data.NotifyListChangedExternally();
		ScrollTo(0);
	}

	protected override void UpdateCellViewsHolder(UIDriverItem newOrRecycled) {
		newOrRecycled.Init(data[newOrRecycled.ItemIndex]);
	}
}

public class UIDriverItem : CellViewsHolder {

	private TextMeshProUGUI nameText;
	private TextMeshProUGUI incomeText;
	private HorizontalLayoutGroup starsGroup;

	private Image image;
	
	private Button hireButton;
	private CanvasGroup hireButtonCanvasGroup;
	private TextMeshProUGUI hireButtonText;
	
	public override void CollectViews() {
		base.CollectViews();
		views.GetComponentAtPath("Top/Top/Name", out nameText);
		views.GetComponentAtPath("Top/Top/IncomeText", out incomeText);
		views.GetComponentAtPath("Top/Stars", out starsGroup);
		views.GetComponentAtPath("Image", out image);
		views.GetComponentAtPath("HireButton", out hireButton);
		views.GetComponentAtPath("HireButton/CanvasGroup", out hireButtonCanvasGroup);
		views.GetComponentAtPath("HireButton/CanvasGroup/Group/Text", out hireButtonText);
	}

	public void Init(DriverDesignData data) {
		nameText.text = data.name;
		incomeText.text = $"+{data.income:N0}";
		GameController.Instance.EndOfFrame(() => {
			HorizontalLayoutGroup horizontalLayoutGroup = incomeText.transform.parent.GetComponent<HorizontalLayoutGroup>();
			horizontalLayoutGroup.enabled = false;
			horizontalLayoutGroup.enabled = true;
		});
		
		for (int i = 0; i < 5; i++) {
			starsGroup.transform.GetChild(i).GetChild(0).gameObject.SetActive(data.stars > i);
		}

		// hireButton.gameObject.SetActive(parkingSlotData.slotUnlocked && !parkingSlotData.taxiPurchased);
		
		image.sprite = Resources.Load<Sprite>($"Company/Drivers/Driver{data.id}");
		hireButtonText.text = data.hireCost.ToString("N0");
		GameController.Instance.EndOfFrame(() => {
			HorizontalLayoutGroup horizontalLayoutGroup = hireButtonText.transform.parent.GetComponent<HorizontalLayoutGroup>();
			horizontalLayoutGroup.enabled = false;
			horizontalLayoutGroup.enabled = true;
		});
	}
}