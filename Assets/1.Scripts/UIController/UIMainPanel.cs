using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class UIMainPanel : UIPanel<UIMainPanel.Data> {
	
	[Space]
	[SerializeField] private RectTransform topContainer;
	[SerializeField] private UICoins coinsPanel;
	[SerializeField] private Button settingsButton;
	
	[Space]
	[SerializeField] private Button driversButton;
	[SerializeField] private Button multiplyCashButton;
	[SerializeField] private Button driveButton;
	
	public UICoins CoinsPanel => coinsPanel;
	
	protected override void OnInit() {
		settingsButton.onClick.AddListener(data.onSettingsButton);
		driversButton.onClick.AddListener(data.onDriversButton);
		multiplyCashButton.onClick.AddListener(data.onMultiplyCashButton);
		driveButton.onClick.AddListener(data.onDriveButton);
	}

	public void ShowSettingsButton(bool show) {
		settingsButton.GetComponent<CanvasGroup>().DOFade(show ? 1f : 0f, UIController.defaultTime);
	}

	public void MoveToOtherPanel(Transform parent) {
		coinsPanel.transform.parent = parent;
		settingsButton.transform.parent = parent;
		settingsButton.GetComponent<RectTransform>().SetAnchorPosY(-140f);
	}

	public void MoveBack() {
		coinsPanel.transform.parent = topContainer;
		settingsButton.transform.parent = topContainer;
		settingsButton.GetComponent<RectTransform>().SetAnchorPosY(-40f);
	}

	public new class Data : UIPanelBase.Data {
		public UnityAction onSettingsButton;
		public UnityAction onDriversButton;
		public UnityAction onMultiplyCashButton;
		public UnityAction onDriveButton;
	}
}
