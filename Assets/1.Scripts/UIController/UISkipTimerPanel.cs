using System;
using UnityEngine;
using UnityEngine.UI;

public class UISkipTimerPanel : UIPanel<UISkipTimerPanel.Data> {

	[SerializeField] private Button watchAdButton;
	
	protected override void OnInit() {
		watchAdButton.onClick.RemoveAllListeners();
		watchAdButton.onClick.AddListener(() => {
			if (Application.isEditor) {
				Close();
				data.onWatchAd();
			} else {
				AdsController.Instance.ShowAd(AdsController.AdType.Rewarded_Android, success => {
					if (success) {
						Close();
						data.onWatchAd();
					}
				}, "floor");	
			}
		});
	}

	public new class Data : UIPanelBase.Data {
		public Action onWatchAd;
	}
}
