using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class UITakeMissionPanel : UIPanel<UITakeMissionPanel.Data> {
    
	[SerializeField] private UIMissionsList missionsList;
	
	protected override void OnInit() {
		this.WaitForFrames(1, () => missionsList.Init(data.itemsData));
	}
	
	protected override void ShowAnim(Action onComplete) {
		gameObject.SetActive(true);
		background.SetAlpha(0f);
		background.DOFade(1f, UIController.defaultTime);
		RectTransform contentRect = content.GetComponent<RectTransform>();
		contentRect.SetAnchorPosY(-1170f);
		contentRect.DOAnchorPosY(-70f, UIController.defaultTime).SetEase(Ease.OutQuad).OnComplete(() => {
			onComplete();
		});
	}

	protected override void CloseAnim(bool anim, Action onComplete) {
		RectTransform contentRect = content.GetComponent<RectTransform>();
		background.DOFade(0f, UIController.defaultTime);
		contentRect.DOAnchorPosY(-1170f, UIController.defaultTime).SetEase(Ease.InQuad).OnComplete(() => {
			gameObject.SetActive(false);
		});
		onComplete();
	}
	
	public new class Data : UIPanelBase.Data {
		public List<UIMissionsList.ItemData> itemsData;
	}
}
