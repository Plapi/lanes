using System;
using UnityEngine;
using DG.Tweening;

public class UIIncomePanel : UIPanel<UIIncomePanel.Data> {

	[SerializeField] private UIIncomeList incomeList;
	
	protected override void OnInit() {
		this.WaitForFrames(1, incomeList.Init);
	}
	
	protected override void ShowAnim(Action onComplete) {
		gameObject.SetActive(true);
		RectTransform contentRect = content.GetComponent<RectTransform>();
		contentRect.SetAnchorPosY(-1170f);
		contentRect.DOAnchorPosY(-70f, UIController.defaultTime).SetEase(Ease.OutQuad).OnComplete(() => {
			onComplete();
			GameController.Instance.OnCoinsUpdate += incomeList.OnCoinsUpdate;
		});
	}

	protected override void CloseAnim(bool anim, Action onComplete) {
		RectTransform contentRect = content.GetComponent<RectTransform>();
		contentRect.DOAnchorPosY(-1170f, UIController.defaultTime).SetEase(Ease.InQuad).OnComplete(() => {
			gameObject.SetActive(false);
		});
		onComplete();
		GameController.Instance.OnCoinsUpdate -= incomeList.OnCoinsUpdate;
	}

	public new class Data : UIPanelBase.Data {
		
	}
}
