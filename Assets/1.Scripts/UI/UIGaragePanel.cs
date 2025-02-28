using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIGaragePanel : UIPanel<UIGaragePanel.Data> {
    
	[Space]
	[SerializeField] private Button leftButton;
	[SerializeField] private Button rightButton;
	[SerializeField] private Button goButton;

	protected override void OnInit() {
		leftButton.onClick.AddListener(data.onLeft);
		rightButton.onClick.AddListener(data.onRight);
		goButton.onClick.AddListener(data.onGo);
	}
	
	public new class Data: UIPanelBase.Data {
		public UnityAction onLeft;
		public UnityAction onRight;
		public UnityAction onGo;
	}
}
