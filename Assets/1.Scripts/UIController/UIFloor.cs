using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIFloor : UIObject {

	[SerializeField] private CanvasGroup canvasGroup;
	
	[Space]
	[SerializeField] private Button topButton;
	[SerializeField] private Button bottomButton;
	[SerializeField] private Image floorImage;
	[SerializeField] private GameObject floorLock;
	[SerializeField] private GameObject floorGlow;
	[SerializeField] private TextMeshProUGUI floorText;

	[Space]
	[SerializeField] private Button upgradeButton;
	[SerializeField] private CanvasGroup upgradeButtonCanvasGroup;
	[SerializeField] private TextMeshProUGUI upgradeButtonText;
	
	[Space]
	[SerializeField] private Button skipTimeButton;
	[SerializeField] private TextMeshProUGUI timeText;
	
	[Space]
	[SerializeField] private Color floorNormalColor;
	[SerializeField] private Color floorDisableColor;
	
	[Space]
	[SerializeField] private AudioClip floorUpgradeCompleteAudioClip;

	private int currentFloor;
	private Timer timer;
	private Action onUpgradeComplete;

	private void Awake() {
		canvasGroup.gameObject.SetActive(false);
		canvasGroup.alpha = 0f;
	}

	public void Init(int floor, Action<int> onUpdate, Action onUpgradeStart, Action onUpgradeComplete) {
		
		currentFloor = floor;
		this.onUpgradeComplete = onUpgradeComplete;
		UpdateFloor();
		
		topButton.onClick.RemoveAllListeners();
		topButton.onClick.AddListener(() => {
			currentFloor++;
			onUpdate(currentFloor);
			UpdateFloor();
		});
		bottomButton.onClick.RemoveAllListeners();
		bottomButton.onClick.AddListener(() => {
			currentFloor--;
			onUpdate(currentFloor);
			UpdateFloor();
		});
		
		upgradeButton.onClick.RemoveAllListeners();
		upgradeButton.onClick.AddListener(() => {
			onUpgradeStart?.Invoke();
			UpdateFloor();
		});
		
		skipTimeButton.onClick.RemoveAllListeners();
		skipTimeButton.onClick.AddListener(() => {
			if (Application.isEditor) {
				UpgradeComplete();
			} else {
				AdsController.Instance.ShowAd(success => {
					if (success) {
						UpgradeComplete();
					}
				});	
			}
		});
	}

	private void UpdateFloor() {
		if (timer != null) {
			Destroy(timer);
		}
		
		int floorReached = PlayerPrefsManager.UserData.GetFlorReached();
		SerializedDateTime floorUpgradeTime = PlayerPrefsManager.UserData.floorUpgradeTime;
		bool floorUnlocked = currentFloor <= floorReached;
		bool inFloorUpgradeTime = PlayerPrefsManager.UserData.inFloorUpgradeTime && currentFloor > floorReached;
		
		floorText.text = currentFloor == 0 ? "P" : currentFloor.ToString();
		topButton.interactable = floorUnlocked && currentFloor < Settings.Instance.company.maxFloors;
		bottomButton.interactable = currentFloor > 0;
		
		floorImage.color = floorUnlocked ? floorNormalColor : floorDisableColor;
		floorGlow.SetActive(floorUnlocked);
		floorLock.SetActive(!floorUnlocked);
		
		GameController.Instance.OnCoinsUpdate -= UpdateUpgradeButton;
		if (!floorUnlocked) {
			if (!inFloorUpgradeTime) {
				if (!upgradeButton.gameObject.activeSelf) {
					ShowButtonAnim(upgradeButton.GetComponent<RectTransform>());
				}
				UpdateUpgradeButton();
				GameController.Instance.OnCoinsUpdate += UpdateUpgradeButton;
			} else {
				if (!upgradeButton.gameObject.activeSelf) {
					ShowButtonAnim(skipTimeButton.GetComponent<RectTransform>());
				}
				timer = Timer.Create(gameObject, 1f, () => {
					DateTime endTime = floorUpgradeTime.Date.AddMinutes(Settings.Instance.company.floorUpgradeDurationMinutes);
					if (endTime > DateTime.Now) {
						TimeSpan remainingTime = endTime - DateTime.Now;
						timeText.text = $"{remainingTime.Minutes:00}:{remainingTime.Seconds:00}";
					} else {
						UpgradeComplete();
					}
				});
			}
		}
		upgradeButton.gameObject.SetActive(!floorUnlocked && !inFloorUpgradeTime);
		skipTimeButton.gameObject.SetActive(!floorUnlocked && inFloorUpgradeTime);
	}

	private void UpdateUpgradeButton() {
		int coins = PlayerPrefsManager.UserData.coins;
		int upgradeCost = PlayerPrefsManager.UserData.GetCurrentFloorUpgradeCost();
		bool canUpgrade = coins >= upgradeCost;
		upgradeButtonText.text = Utils.FormatInt(upgradeCost);
		upgradeButton.interactable = canUpgrade;
		upgradeButtonCanvasGroup.alpha = canUpgrade ? 1f : 0.7f;
		GameController.Instance.EndOfFrame(() => {
			HorizontalLayoutGroup horizontalLayoutGroup = upgradeButtonText.transform.parent.GetComponent<HorizontalLayoutGroup>();
			horizontalLayoutGroup.enabled = false;
			horizontalLayoutGroup.enabled = true;
		});
	}

	private void UpgradeComplete() {
		onUpgradeComplete?.Invoke();
		UpdateFloor();
		AudioSystem.Play(floorUpgradeCompleteAudioClip);
	}

	public void UpdateVisibility(bool visible) {
		canvasGroup.DOKill();
		if (visible) {
			canvasGroup.gameObject.SetActive(true);
		}
		canvasGroup.DOFade(visible ? 1f : 0f, UIController.defaultTime).OnComplete(() => {
			if (!visible) {
				currentFloor = PlayerPrefsManager.UserData.GetFlorReached();
				UpdateFloor();
				canvasGroup.gameObject.SetActive(false);
			}
		});
	}
	
	private static void ShowButtonAnim(RectTransform rectTransform) {
		rectTransform.DOKill();
		rectTransform.SetAnchorPosX(-35f);
		rectTransform.DOAnchorPosX(-115f, UIController.defaultTime).SetEase(Ease.OutExpo);
	}
}
