using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIMainPanel : UIPanel<UIMainPanel.Data> {
	
	[Space]
	[SerializeField] private Button driversButton;
	[SerializeField] private Button multiplyCashButton;
	[SerializeField] private Button driveButton;
	
	protected override void OnInit() {
		driversButton.onClick.AddListener(data.onDriversButton);
		multiplyCashButton.onClick.AddListener(data.onMultipleCashButton);
		driveButton.onClick.AddListener(data.ondriveButton);
	}

	public new class Data : UIPanelBase.Data {
		public UnityAction onDriversButton;
		public UnityAction onMultipleCashButton;
		public UnityAction ondriveButton;
	}
}
