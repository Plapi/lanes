using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public abstract class UIPanel<T> : UIPanelBase where T: UIPanelBase.Data {

	[SerializeField] protected Image background;
	[SerializeField] protected CanvasGroup content;
	[SerializeField] protected Button[] closeButtons;

	protected T data { get; private set; }
	
	public void Init(T data) {
		this.data = data;

		for (int i = 0; i < closeButtons.Length; i++) {
			closeButtons[i].onClick.RemoveAllListeners();
			closeButtons[i].onClick.AddListener(() => Close());
		}
		
		OnInit();
	}

	protected abstract void OnInit();
	
	public void Show() {
		ShowAnim(() => {
			data.onShow?.Invoke();
		});
	}

	public void Close(bool anim = true) {
		CloseAnim(anim, () => {
			data.onClose?.Invoke();		
		});
	}

	protected virtual void ShowAnim(Action onComplete) {
		gameObject.SetActive(true);
		if (background != null && content != null) {
			OnShowAnimBegin?.Invoke();
			background.SetAlpha(0f);
			background.DOFade(1f, 0.2f).OnComplete(() => {
				OnShowAnimEnd?.Invoke();
				onComplete();
			}).SetUpdate(true);
			content.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f).SetUpdate(true);
		} else {
			onComplete();
		}
	}
	
	protected virtual void CloseAnim(bool anim, Action onComplete) {
		if (anim && background != null && content != null) {
			OnCloseAnimBegin?.Invoke();
			background.DOFade(0f, 0.2f).OnComplete(() => {
				gameObject.SetActive(false);
				OnCloseAnimEnd?.Invoke();
				onComplete();
			}).SetUpdate(true);
			content.transform.DOScale(Vector3.one * 0.5f, 0.2f).SetEase(Ease.InQuad).OnComplete(() => {
				content.transform.localScale = Vector3.one;
			}).SetUpdate(true);
			content.DOFade(0f, 0.2f).OnComplete(() => {
				content.alpha = 1f;
			}).SetUpdate(true);
		} else {
			gameObject.SetActive(false);
			onComplete();
		}
	}
}

public abstract class UIPanelBase : UIObject {
	
	public Action OnShowAnimBegin;
	public Action OnShowAnimEnd;
	public Action OnCloseAnimBegin;
	public Action OnCloseAnimEnd;
	
	public abstract class Data {
		public UnityAction onShow;
		public UnityAction onClose;
	}
}


