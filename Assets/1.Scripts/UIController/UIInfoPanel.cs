using UnityEngine;
using TMPro;

public class UIInfoPanel : UIPanel<UIInfoPanel.Data> {

	[SerializeField] private TextMeshProUGUI titleText;
	[SerializeField] private TextMeshProUGUI descriptionText;
	
	protected override void OnInit() {
		titleText.text = data.title;
		descriptionText.text = data.description;
	}
	
	public new class Data: UIPanelBase.Data {
		public string title;
		public string description;
	}
}
