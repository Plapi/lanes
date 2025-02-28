using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UITopPanel : UIPanel<UITopPanel.Data> {

	[Space]
	[SerializeField] private Button pauseButton;

	protected override void OnInit() {
		pauseButton.onClick.AddListener(data.onPause);
	}

	protected override void ShowAnim(Action onComplete) {
		gameObject.SetActive(true);
		RectTransform.SetAnchorPosY(150f);
		RectTransform.DOAnchorPosY(0f, 0.2f).SetEase(Ease.OutQuad).OnComplete(() => {
			onComplete?.Invoke();
		});
	}

	protected override void CloseAnim(bool anim, Action onComplete) {
		if (anim) {
			RectTransform.DOAnchorPosY(150f, 0.2f).SetEase(Ease.InQuad).OnComplete(() => {
				gameObject.SetActive(false);
				onComplete?.Invoke();
			});	
		} else {
			gameObject.SetActive(true);
			onComplete?.Invoke();
		}
	}

	public new class Data: UIPanelBase.Data {
		public UnityAction onPause;
	}
}
