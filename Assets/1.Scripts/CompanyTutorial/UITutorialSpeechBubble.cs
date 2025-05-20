using System;
using UnityEngine;
using DG.Tweening;

public class UITutorialSpeechBubble : UIObject {

	[SerializeField] private GameObject arrow;
	
	public void Show(bool arrowIsActive) {
		gameObject.SetActive(true);
		arrow.SetActive(arrowIsActive);
		RectTransform.localScale = Vector3.one * 0.01f;
		RectTransform.DOScale(Vector3.one, 0.2f);
	}

	public void Hide() {
		RectTransform.GetComponent<CanvasGroup>().DOFade(0f, 0.2f).OnComplete(() => {
			gameObject.SetActive(false);
		});
	}
}
