using System;
using UnityEngine;
using DG.Tweening;

public class UIDriversPanel : UIPanel<UIDriversPanel.Data> {

	[SerializeField] private UIDriversList driversList;
	
	protected override void OnInit() {
		driversList.Init(data.drivers);
	}
	
	protected override void ShowAnim(Action onComplete) {
		gameObject.SetActive(true);
		RectTransform contentRect = content.GetComponent<RectTransform>();
		contentRect.SetAnchorPosY(-800f);
		contentRect.DOAnchorPosY(-70f, UIController.defaultTime).SetEase(Ease.OutQuad).OnComplete(() => onComplete());
	}

	protected override void CloseAnim(bool anim, Action onComplete) {
		RectTransform contentRect = content.GetComponent<RectTransform>();
		contentRect.DOAnchorPosY(-800f, UIController.defaultTime).SetEase(Ease.InQuad).OnComplete(() => {
			gameObject.SetActive(false);
		});
		onComplete();
	}

	public new class Data : UIPanelBase.Data {
		public DriverDesignData[] drivers;
	}
}
