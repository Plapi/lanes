using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UITopPanel : UIPanel<UITopPanel.Data> {

	[Space]
	[SerializeField] private Button pauseButton;

	protected override void OnInit() {
		pauseButton.onClick.AddListener(data.onPause);
	}
	
	public new class Data: UIPanelBase.Data {
		public UnityAction onPause;
	}
	
}
