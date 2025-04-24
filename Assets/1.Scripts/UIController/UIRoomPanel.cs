using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIRoomPanel : UIPanel<UIRoomPanel.Data> {

	[SerializeField] private TextMeshProUGUI titleText;
	[SerializeField] private TextMeshProUGUI levelText;
	[SerializeField] private TextMeshProUGUI descriptionText;
	
	[Space]
	[SerializeField] private Slider levelSlider;
	[SerializeField] private Slider levelUpgradeSlider;
	
	[Space]
	[SerializeField] private TextMeshProUGUI upgradeCostText;
	[SerializeField] private Button upgradeButton;
	[SerializeField] private CanvasGroup upgradeButtonCanvasGroup;

	[Space]
	[SerializeField] private GameObject incomeObj;
	[SerializeField] private TextMeshProUGUI incomeText;
	[SerializeField] private GameObject vaultObj;
	[SerializeField] private TextMeshProUGUI vaultText;
	
	protected override void OnInit() {
		
		titleText.text = data.roomData.design.name;
		levelText.text = data.roomData.level.ToString();

		bool maxLevelReached = data.roomData.MaxLevelReached;
		upgradeButton.gameObject.SetActive(!maxLevelReached);
		
		if (!maxLevelReached) {
			levelSlider.value = (float)(data.roomData.level - 1) / data.roomData.MaxLevel;
			levelUpgradeSlider.value = (float)data.roomData.level / data.roomData.MaxLevel;
		} else {
			levelSlider.value = levelUpgradeSlider.value = 1f;
		}

		VaultRoomData vaultRoomData = data.roomData as VaultRoomData;
		bool isVaultRoom = vaultRoomData != null;
		incomeObj.SetActive(!isVaultRoom);
		vaultObj.SetActive(isVaultRoom);
		if (isVaultRoom) {
			vaultText.text = $"{PlayerPrefsManager.UserData.coins:N0}/\n{vaultRoomData.Capacity:N0}";
			if (!maxLevelReached) {
				descriptionText.text = $"Upgrade {data.roomData.design.name} to <color=#30B8FF>Level {data.roomData.level + 1},</color>\n" +
				                       "to increase <color=#30B8FF>storage capacity.</color>";
			} else {
				descriptionText.text = $"<size=40>{data.roomData.design.name} is <color=#30B8FF>fully upgraded!</color></size><line-height=80>\n" +
				                       "You’ve unlocked the maximum\n<color=#30B8FF>storage capacity</color>!";
			}
		} else {
			incomeText.text = $"+{data.roomData.design.cashIncomes[data.roomData.level - 1]:N0}";
			if (!maxLevelReached) {
				descriptionText.text = $"Upgrade {data.roomData.design.name} to <color=#30B8FF>Level {data.roomData.level + 1},</color>\n" +
				                       "to increase <color=#30B8FF>cash income.</color>";
			} else {
				descriptionText.text = $"<size=40>{data.roomData.design.name} is <color=#30B8FF>fully upgraded!</color></size><line-height=80>\n" +
				                       "<line-height=40>Great job! You've maximized\n<color=#30B8FF>cash income</color> from this room.";
			}	
		}

		int coins = PlayerPrefsManager.UserData.coins;
		int upgradeCost = data.roomData.design.upgradeCosts[data.roomData.level - 1];
		upgradeCostText.text = upgradeCost.ToString("N0");
		this.EndOfFrame(() => {
			HorizontalLayoutGroup horizontalLayoutGroup = upgradeCostText.transform.parent.GetComponent<HorizontalLayoutGroup>();
			horizontalLayoutGroup.enabled = false;
			horizontalLayoutGroup.enabled = true;
		});
		bool canUpgrade = !maxLevelReached && coins >= upgradeCost;
		
		upgradeButton.interactable = canUpgrade;
		upgradeButton.onClick.RemoveAllListeners();
		upgradeButton.onClick.AddListener(() => {
			PlayerPrefsManager.UserData.coins -= upgradeCost;
			data.roomData.level++;
			PlayerPrefsManager.SaveUserData();
			data.onUpgrade?.Invoke();
		});
		upgradeButtonCanvasGroup.alpha = canUpgrade ? 1f : 0.5f;
	}

	protected override void ShowAnim(Action onComplete) {
		gameObject.SetActive(true);
		RectTransform contentRect = content.GetComponent<RectTransform>();
		contentRect.SetAnchorPosY(-800f);
		contentRect.DOAnchorPosY(-70f, UIController.defaultTime).SetEase(Ease.OutQuad).OnComplete(() => onComplete());
	}

	protected override void CloseAnim(bool anim, Action onComplete) {
		RectTransform contentRect = content.GetComponent<RectTransform>();
		contentRect.DOAnchorPosY(-800f, UIController.defaultTime).SetEase(Ease.InQuad).OnComplete(() => {
			gameObject.SetActive(false);
		});
		onComplete();
	}

	public new class Data: UIPanelBase.Data {
		public RoomData roomData;
		public Action onUpgrade;
	}
}
