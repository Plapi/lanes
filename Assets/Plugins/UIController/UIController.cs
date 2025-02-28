using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIController : MonoBehaviourSingleton<UIController> {

	public const float defaultTime = 0.2f;
	
	[SerializeField] private UIPanelBase[] panels;
	[SerializeField] private GameObject touchBlocker;
	[SerializeField] private Image fadeToBlackImage;
	
	private readonly Dictionary<Type, UIPanelBase> dictPanels = new();

	protected override void Awake() {
		base.Awake();
		for (int i = 0; i < panels.Length; i++) {
			dictPanels.Add(panels[i].GetType(), panels[i]);
			panels[i].OnShowAnimBegin = () => {
				touchBlocker.SetActive(true);
			};
			panels[i].OnShowAnimEnd = () => {
				touchBlocker.SetActive(false);
			};
			panels[i].OnCloseAnimBegin = () => {
				touchBlocker.SetActive(true);
			};
			panels[i].OnCloseAnimEnd = () => {
				touchBlocker.SetActive(false);
			};
		}
	}

	public T GetPanel<T>() where T : UIPanelBase {
		return dictPanels[typeof(T)] as T;
	}

	public void FadeInToBlack(Action onComplete = null) {
		fadeToBlackImage.SetAlpha(0f);
		fadeToBlackImage.gameObject.SetActive(true);
		fadeToBlackImage.DOFade(1f, 0.2f).OnComplete(() => {
			onComplete?.Invoke();
		}).SetUpdate(true);
	}
	
	public void FadeOutToBlack(Action onComplete = null) {
		fadeToBlackImage.DOFade(0f, 0.2f).OnComplete(() => {
			fadeToBlackImage.gameObject.SetActive(false);
			onComplete?.Invoke();
		}).SetUpdate(true);
	}
}
