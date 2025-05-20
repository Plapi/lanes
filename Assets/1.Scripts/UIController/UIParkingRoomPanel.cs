using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Coffee.UIExtensions;

public class UIParkingRoomPanel : UIPanel<UIParkingRoomPanel.Data> {

	[SerializeField] private TextMeshProUGUI levelText;
	[SerializeField] private TextMeshProUGUI descriptionText;
	[SerializeField] private TextMeshProUGUI taxiText;
	
	[Space]
	[SerializeField] private Slider levelSlider;
	[SerializeField] private Slider levelUpgradeSlider;
	
	[Space]
	[SerializeField] private UIParkingList parkingList;
	
	[Space]
	[SerializeField] private TextMeshProUGUI upgradeCostText;
	[SerializeField] private Button upgradeButton;
	[SerializeField] private CanvasGroup upgradeButtonCanvasGroup;
	
	[Space]
	[SerializeField] private AudioClip upgradeClip;
	[SerializeField] private UIParticle upgradeParticle;

	[Space]
	[SerializeField] private GameObject tutorialObj;
	[SerializeField] private Button tutorialBuyTaxiButton;
	
	protected override void OnInit() {
		levelText.text = data.roomData.level.ToString();
		
		bool maxLevelReached = data.roomData.MaxLevelReached;
		upgradeButton.gameObject.SetActive(!maxLevelReached);
		
		if (!maxLevelReached) {
			levelSlider.value = (float)(data.roomData.level - 1) / data.roomData.MaxLevel;
			levelUpgradeSlider.value = (float)data.roomData.level / data.roomData.MaxLevel;
		} else {
			levelSlider.value = levelUpgradeSlider.value = 1f;
		}
		
		GameController.Instance.EndOfFrame(() => {
			HorizontalLayoutGroup horizontalLayoutGroup = taxiText.transform.parent.GetComponent<HorizontalLayoutGroup>();
			horizontalLayoutGroup.enabled = false;
			horizontalLayoutGroup.enabled = true;
		});
		
		parkingList.Init(data.roomData.parkingSlots, parkingSlotData => {
			data.onBuyTaxi?.Invoke(parkingSlotData);
		}, parkingSlotData => {
			data.onAssignDriver?.Invoke(parkingSlotData);
		});
		
		if (!maxLevelReached) {
			descriptionText.text = $"Upgrade Parking to <color=#30B8FF>Level {data.roomData.level + 1},</color>\n" +
			                       "to unlock a new <color=#30B8FF>parking slot.</color>";
		} else {
			descriptionText.text = "<size=40>Congratulations!</size><line-height=80>\n" +
			                       "<line-height=40>You've unlocked all <color=#30B8FF>parking slots</color>\n" +
			                       "and expanded your fleet to the <color=#30B8FF>max!</color>";
		}

		UpdateUpgradeButton();
	}
	
	private void UpdateUpgradeButton() {
		taxiText.text = $"{data.roomData.GetTotalTaxiCount()}/{data.roomData.level}";
		
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
	
	private void PlayUpgrade() {
		AudioSystem.Play(upgradeClip);
		upgradeParticle.Play();
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
		GameController.Instance.OnCoinsUpdate += UpdateUpgradeButton;
	}

	protected override void CloseAnim(bool anim, Action onComplete) {
		RectTransform contentRect = content.GetComponent<RectTransform>();
		contentRect.DOAnchorPosY(-800f, UIController.defaultTime).SetEase(Ease.InQuad).OnComplete(() => {
			gameObject.SetActive(false);
			GameController.Instance.OnCoinsUpdate -= UpdateUpgradeButton;
		});
		onComplete();
	}

	public new class Data: UIPanelBase.Data {
		public ParkingRoomData roomData;
		public Action onUpgrade;
		public Action<ParkingSlotData> onBuyTaxi;
		public Action<ParkingSlotData> onAssignDriver;
	}
}
