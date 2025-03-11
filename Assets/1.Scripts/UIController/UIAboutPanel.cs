using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIAboutPanel : UIPanel<UIAboutPanel.Data> {
	
	[SerializeField] private Button mailButton;
	
	protected override void OnInit() {
		mailButton.onClick.RemoveAllListeners();
		mailButton.onClick.AddListener(data.onMail);
	}

	public new class Data : UIPanelBase.Data {
		public UnityAction onMail;
	}
}