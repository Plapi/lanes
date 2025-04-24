using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class UIMainPanel : UIPanel<UIMainPanel.Data> {

	[Space]
	[SerializeField] private UICoins coinsPanel;
	[SerializeField] private Button settingsButton;
	
	[Space]
	[SerializeField] private Button driversButton;
	[SerializeField] private Button multiplyCashButton;
	[SerializeField] private Button driveButton;
	
	public UICoins CoinsPanel => coinsPanel;
	
	protected override void OnInit() {
		coinsPanel.UpdateCoins(data.coins, data.income);
		settingsButton.onClick.AddListener(data.onSettingsButton);
		driversButton.onClick.AddListener(data.onDriversButton);
		multiplyCashButton.onClick.AddListener(data.onMultipleCashButton);
		driveButton.onClick.AddListener(data.onDriveButton);
	}

	public void ShowSettingsButton(bool show) {
		settingsButton.GetComponent<CanvasGroup>().DOFade(show ? 1f : 0f, UIController.defaultTime);
	}

	public new class Data : UIPanelBase.Data {
		public int coins;
		public int income;
		public UnityAction onSettingsButton;
		public UnityAction onDriversButton;
		public UnityAction onMultipleCashButton;
		public UnityAction onDriveButton;
	}
}
