using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIPausePanel : UIPanel<UIPausePanel.Data> {

	[SerializeField] private Button settingsButton;
	[SerializeField] private Button restartButton;
	
	protected override void OnInit() {
		settingsButton.onClick.AddListener(data.onSettings);
		restartButton.onClick.AddListener(data.onRestart);
	}
	
	public new class Data: UIPanelBase.Data {
		public UnityAction onSettings;
		public UnityAction onRestart;
	}
	
}