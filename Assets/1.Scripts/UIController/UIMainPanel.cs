using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class UIMainPanel : UIPanel<UIMainPanel.Data> {
	
	[Space]
	[SerializeField] private RectTransform topContainer;
	[SerializeField] private UICoins coinsPanel;
	[SerializeField] private GameObject tutorialArrow;
	[SerializeField] private GameObject tutorialTap;
	
	[Space]
	[SerializeField] private RectTransform bottomContainer;
	[SerializeField] private Button shopButton;
	[SerializeField] private Button driversButton;
	[SerializeField] private Button multiplyCashButton;
	[SerializeField] private Button driveButton;
	[SerializeField] private Button settingsButton;

	[Space]
	[SerializeField] private GameObject[] boostIncomeObjects;
	
	[Space]
	[SerializeField] private UIFloor floorPanel;
	
	public UIFloor FloorPanel => floorPanel;
	
	public UICoins CoinsPanel => coinsPanel;
	
	protected override void OnInit() {
		coinsPanel.GetComponent<Button>().onClick.AddListener(data.onCoinsButton);
		shopButton.onClick.AddListener(data.onShopButton);
		driversButton.onClick.AddListener(data.onDriversButton);
		multiplyCashButton.onClick.AddListener(data.onMultiplyCashButton);
		driveButton.onClick.AddListener(data.onDriveButton);
		settingsButton.onClick.AddListener(data.onSettingsButton);
	}

	public void MoveToOtherPanel(Transform parent) {
		coinsPanel.transform.SetParent(parent);
	}

	public void MoveBack() {
		coinsPanel.transform.SetParent(topContainer);
	}

	public void HideForTutorial() {
		topContainer.gameObject.SetActive(false);
		bottomContainer.gameObject.SetActive(false);
	}
	
	public void ShowAfterTutorial() {
		bottomContainer.gameObject.SetActive(true);
		bottomContainer.SetAnchorPosY(-200f);
		bottomContainer.DOAnchorPosY(0f, 0.5f).SetEase(Ease.OutExpo);
	}

	public void ShowTopForTutorial() {
		topContainer.gameObject.SetActive(true);
		tutorialArrow.SetActive(true);
	}

	public void HideTutorialArrow() {
		tutorialArrow.SetActive(false);
	}

	public void ShowTutorialTap() {
		tutorialTap.SetActive(true);
	}

	public void HideTutorialTap() {
		tutorialTap.SetActive(false);
	}

	public void UpdateBoostIncomeObjects() {
		bool active = PlayerPrefsManager.UserData.InWatchAdBoostIncome();
		if (boostIncomeObjects[0].activeSelf != active) {
			for (int i = 0; i < boostIncomeObjects.Length; i++) {
				boostIncomeObjects[i].SetActive(active);
			}
		}
		bool removeAdsPurchased = PlayerPrefsManager.UserData.removeAdsPurchased;
		if (multiplyCashButton.interactable == removeAdsPurchased) {
			multiplyCashButton.interactable = !removeAdsPurchased;	
		}
	}

	public new class Data : UIPanelBase.Data {
		public UnityAction onCoinsButton;
		public UnityAction onShopButton;
		public UnityAction onDriversButton;
		public UnityAction onMultiplyCashButton;
		public UnityAction onDriveButton;
		public UnityAction onSettingsButton;
	}
}
