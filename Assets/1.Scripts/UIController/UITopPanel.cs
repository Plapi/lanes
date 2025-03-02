using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UITopPanel : UIPanel<UITopPanel.Data> {

	[Space]
	[SerializeField] private Button pauseButton;
	
	[Space]
	[SerializeField] private Slider healthSlider;
	[SerializeField] private Image healthFillGreen;
	[SerializeField] private Image healthFillRed;
	
	protected override void OnInit() {
		pauseButton.onClick.AddListener(data.onPause);
	}

	public void ResetHealthSlider() {
		DOTween.Kill(healthSlider);
		DOTween.Kill(healthSlider.transform);
		healthSlider.value = 1f;
	}

	public void UpdateHealthSlider(float value) {
		DOTween.Kill(healthSlider);
		DOTween.Kill(healthSlider.transform);
		healthSlider.DOValue(value, UIController.defaultTime).SetEase(Ease.OutCubic).OnUpdate(() => {
			healthFillGreen.SetAlpha(healthSlider.value);
			healthFillRed.SetAlpha(1f - healthSlider.value);
		});
		healthSlider.transform.DOPunchScale(Vector3.one * 0.2f, UIController.defaultTime).SetUpdate(true);
	}

	protected override void ShowAnim(Action onComplete) {
		gameObject.SetActive(true);
		RectTransform.SetAnchorPosY(150f);
		RectTransform.DOAnchorPosY(0f, UIController.defaultTime).SetEase(Ease.OutQuad).OnComplete(() => {
			onComplete?.Invoke();
		});
	}

	protected override void CloseAnim(bool anim, Action onComplete) {
		if (anim) {
			RectTransform.DOAnchorPosY(150f, UIController.defaultTime).SetEase(Ease.InQuad).OnComplete(() => {
				gameObject.SetActive(false);
				onComplete?.Invoke();
			});	
		} else {
			gameObject.SetActive(false);
			onComplete?.Invoke();
		}
	}

	public new class Data: UIPanelBase.Data {
		public UnityAction onPause;
	}
}
