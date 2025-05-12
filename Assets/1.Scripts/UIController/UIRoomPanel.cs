using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Coffee.UIExtensions;

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

	[Space]
	[SerializeField] private AudioClip upgradeClip;
	[SerializeField] private UIParticle upgradeParticle;
	
	protected override void OnInit() {
		
		titleText.text = data.roomData.Design.name;
		levelText.text = data.roomData.level.ToString();

		bool maxLevelReached = data.roomData.MaxLevelReached;
		upgradeButton.gameObject.SetActive(!maxLevelReached);
		
		if (!maxLevelReached) {
			levelSlider.value = (float)(data.roomData.level - 1) / data.roomData.MaxLevel;
			levelUpgradeSlider.value = (float)data.roomData.level / data.roomData.MaxLevel;
		} else {
			levelSlider.value = levelUpgradeSlider.value = 1f;
		}

		bool isVaultRoom = data.roomData is VaultRoomData;
		incomeObj.SetActive(!isVaultRoom);
		vaultObj.SetActive(isVaultRoom);
		if (isVaultRoom) {
			UpdateVaultCoins();
			if (!maxLevelReached) {
				descriptionText.text = $"Upgrade {data.roomData.Design.name} to <color=#30B8FF>Level {data.roomData.level + 1},</color>\n" +
				                       "to increase <color=#30B8FF>storage capacity.</color>";
			} else {
				descriptionText.text = $"<size=40>{data.roomData.Design.name} is <color=#30B8FF>fully upgraded!</color></size><line-height=80>\n" +
				                       "<line-height=40>You’ve unlocked the maximum\n<color=#30B8FF>storage capacity</color>!";
			}
		} else {
			incomeText.text = $"+{Utils.FormatInt(data.roomData.CoinsIncome)}";
			this.EndOfFrame(() => {
				HorizontalLayoutGroup horizontalLayoutGroup = incomeText.transform.parent.GetComponent<HorizontalLayoutGroup>();
				horizontalLayoutGroup.enabled = false;
				horizontalLayoutGroup.enabled = true;
			});
			if (!maxLevelReached) {
				descriptionText.text = $"Upgrade {data.roomData.Design.name} to <color=#30B8FF>Level {data.roomData.level + 1},</color>\n" +
				                       "to increase <color=#30B8FF>cash income.</color>";
			} else {
				descriptionText.text = $"<size=40>{data.roomData.Design.name} is <color=#30B8FF>fully upgraded!</color></size><line-height=80>\n" +
				                       "<line-height=40>Great job! You've maximized\n<color=#30B8FF>cash income</color> from this room.";
			}
		}
		UpdateUpgradeButton();
	}

	private void UpdateUpgradeButton() {
		if (!upgradeButton.gameObject.activeSelf || data.roomData.MaxLevelReached) {
			return;
		}
		
		int upgradeCost = data.roomData.UpgradeCost;
		upgradeCostText.text = Utils.FormatInt(upgradeCost);
		this.EndOfFrame(() => {
			HorizontalLayoutGroup horizontalLayoutGroup = upgradeCostText.transform.parent.GetComponent<HorizontalLayoutGroup>();
			horizontalLayoutGroup.enabled = false;
			horizontalLayoutGroup.enabled = true;
		});
		
		int coins = PlayerPrefsManager.UserData.coins;
		bool canUpgrade = !data.roomData.MaxLevelReached && coins >= upgradeCost;
		upgradeButton.interactable = canUpgrade;
		upgradeButton.onClick.RemoveAllListeners();
		upgradeButton.onClick.AddListener(() => {
			data.onUpgrade?.Invoke();
			OnInit();
			PlayUpgrade();
		});
		upgradeButtonCanvasGroup.alpha = canUpgrade ? 1f : 0.5f;
	}

	private void UpdateVaultCoins() {
		if (data.roomData is not VaultRoomData vaultRoomData) {
			return;
		}
		vaultText.text = $"{Utils.FormatInt(vaultRoomData.depositedCoins)}/\n{Utils.FormatInt(vaultRoomData.Capacity)}";
		this.EndOfFrame(() => {
			HorizontalLayoutGroup horizontalLayoutGroup = vaultText.transform.parent.GetComponent<HorizontalLayoutGroup>();
			horizontalLayoutGroup.enabled = false;
			horizontalLayoutGroup.enabled = true;
		});
	}

	private void OnCoinsUpdate() {
		UpdateUpgradeButton();
		UpdateVaultCoins();
	}

	private void PlayUpgrade() {
		AudioSystem.Play(upgradeClip);
		upgradeParticle.Play();
		levelText.transform.parent.DOKill();
		levelText.transform.parent.localScale = Vector3.one;
		levelText.transform.parent.DOPunchScale(Vector3.one * 0.2f, UIController.defaultTime).SetUpdate(true);
		levelText.DOColor(Color.green, 0.25f).OnComplete(() => {
			levelText.DOColor(Color.white, 0.25f);
		});
	}

	protected override void ShowAnim(Action onComplete) {
		gameObject.SetActive(true);
		RectTransform contentRect = content.GetComponent<RectTransform>();
		contentRect.SetAnchorPosY(-800f);
		contentRect.DOAnchorPosY(-70f, UIController.defaultTime).SetEase(Ease.OutQuad).OnComplete(() => onComplete());
		GameController.Instance.OnCoinsUpdate += OnCoinsUpdate;
	}

	protected override void CloseAnim(bool anim, Action onComplete) {
		RectTransform contentRect = content.GetComponent<RectTransform>();
		contentRect.DOAnchorPosY(-800f, UIController.defaultTime).SetEase(Ease.InQuad).OnComplete(() => {
			gameObject.SetActive(false);
			GameController.Instance.OnCoinsUpdate -= OnCoinsUpdate;
		});
		onComplete();
	}

	public new class Data: UIPanelBase.Data {
		public RoomData roomData;
		public Action onUpgrade;
	}
}
