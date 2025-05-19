using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using frame8.Logic.Misc.Other.Extensions;
using Com.ForbiddenByte.OSA.Core;
using Com.ForbiddenByte.OSA.CustomParams;
using Com.ForbiddenByte.OSA.DataHelpers;
using TMPro;


public class UIMissionsList : OSA<BaseParamsWithPrefab, UIMissionsList.ItemViewsHolder> {

	private SimpleDataHelper<object> data;

	public void Init(List<ItemData> persons) {
		data = new SimpleDataHelper<object>(this);
		for (int i = 0; i < persons.Count; i++) {
			data.List.Add(persons[i]);
		}
		List<CompletedMission> completedMissions = PlayerPrefsManager.UserData.completedMissions;
		if (completedMissions.Count > 0) {
			data.List.Add(null);
			data.List.AddRange(completedMissions);
		}
		_Params.ItemPrefab.gameObject.SetActive(false);
		data.NotifyListChangedExternally();
	}

	protected override ItemViewsHolder CreateViewsHolder(int itemIndex) {
		ItemViewsHolder instance = new();
		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
		return instance;
	}
	
	protected override void UpdateViewsHolder(ItemViewsHolder newOrRecycled) {
		newOrRecycled.Init(data[newOrRecycled.ItemIndex]);
	}

	public class ItemData {
		public RideController.CurrentPerson person;
		public int coins;
		public int intersections;
		public Action<ItemData> onSelect;
	}
	
	public class ItemViewsHolder : BaseItemViewsHolder {
		public void Init(object obj) {
			if (obj is ItemData itemData) {
				root.HideAllChildrenExcept(0);
				root.GetComponentAtPath("Mission/MaskIcon/Icon", out Image icon);
				root.GetComponentAtPath("Mission/Group/NameText", out TextMeshProUGUI nameText);
				root.GetComponentAtPath("Mission/Group/CoinsText", out TextMeshProUGUI coinsText);
				root.GetComponentAtPath("Mission/SelectButton", out Button selectButton);
				icon.sprite = itemData.person.GetSprite();
				nameText.text = Settings.Instance.personNames[itemData.person.group * 9 + itemData.person.index];
				coinsText.text = $" {Utils.FormatInt(itemData.coins)}";
				GameController.Instance.EndOfFrame(() => {
					HorizontalLayoutGroup horizontalLayoutGroup = coinsText.transform.parent.GetComponent<HorizontalLayoutGroup>();
					horizontalLayoutGroup.enabled = false;
					horizontalLayoutGroup.enabled = true;
				});
				selectButton.onClick.RemoveAllListeners();
				selectButton.onClick.AddListener(() => itemData.onSelect(itemData));
			} else if (obj is CompletedMission completedMission) {
				root.HideAllChildrenExcept(1);
				root.GetComponentAtPath("CompletedMission/MaskIcon/Icon", out Image icon);
				root.GetComponentAtPath("CompletedMission/NameText", out TextMeshProUGUI nameText);
				root.GetComponentAtPath("CompletedMission/Stars", out Transform stars);
				icon.sprite = completedMission.person.GetSprite();
				nameText.text = Settings.Instance.personNames[completedMission.person.group * 9 + completedMission.person.index];
				for (int i = 0; i < stars.childCount; i++) {
					stars.GetChild(i).GetChild(0).gameObject.SetActive(completedMission.stars <= i);
					stars.GetChild(i).GetChild(1).gameObject.SetActive(completedMission.stars > i);
				}
			} else {
				root.HideAllChildrenExcept(2);
			}
		}
	}
}
