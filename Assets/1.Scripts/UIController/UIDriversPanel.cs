using System;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class UIDriversPanel : UIPanel<UIDriversPanel.Data> {

	[SerializeField] private UIDriversList driversList;
	[SerializeField] private TextMeshProUGUI title;
	
	protected override void OnInit() {
		driversList.Init(data.drivers, data.onHire, data.onFire, data.onSelect);
		title.text = data.onSelect != null ? "Select Driver" : "Drivers";
	}
	
	protected override void ShowAnim(Action onComplete) {
		gameObject.SetActive(true);
		RectTransform contentRect = content.GetComponent<RectTransform>();
		contentRect.SetAnchorPosY(-1170f);
		contentRect.DOAnchorPosY(-70f, UIController.defaultTime).SetEase(Ease.OutQuad).OnComplete(() => onComplete());
	}

	protected override void CloseAnim(bool anim, Action onComplete) {
		RectTransform contentRect = content.GetComponent<RectTransform>();
		contentRect.DOAnchorPosY(-1170f, UIController.defaultTime).SetEase(Ease.InQuad).OnComplete(() => {
			gameObject.SetActive(false);
		});
		onComplete();
	}

	public new class Data : UIPanelBase.Data {
		public DriverData[] drivers;
		public Action<DriverData> onHire;
		public Action<DriverData> onFire;
		public Action<DriverData> onSelect;
	}
}
