using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIChangeColor : UIObject {

	[SerializeField] private Button button;
	[SerializeField] private Button closeButton;
	[SerializeField] private GameObject scrollView;
	[SerializeField] private RectTransform content;
	[SerializeField] private List<UIChangeColorItem> items;

	public void Init(Color[] colors, int selection, Action<int> onSelect) {

		scrollView.gameObject.SetActive(false);

		void toggleScrollView() {
			bool active = !scrollView.gameObject.activeSelf;
			if (active) {
				ShowAnim(selection);
			} else {
				HideAnim();
			}
		}
		
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(toggleScrollView);
		closeButton.onClick.RemoveAllListeners();
		closeButton.onClick.AddListener(toggleScrollView);
		
		for (int i = items.Count; i < colors.Length; i++) {
			UIChangeColorItem item = Instantiate(items[0], items[0].transform.parent);
			item.name = $"Item{i}";
			items.Add(item);
		}

		for (int i = 0; i < items.Count; i++) {
			items[i].gameObject.SetActive(i < colors.Length);
		}

		for (int i = 0; i < colors.Length; i++) {
			int index = i;
			items[i].Init(colors[i], i == selection, () => {
				selection = index;
				ActivateToggle(index);
				onSelect?.Invoke(index);
			});
		}
	}

	private void OnDisable() {
		scrollView.gameObject.SetActive(false);
	}

	private void ShowAnim(int selection) {
		scrollView.gameObject.SetActive(true);
		
		this.WaitForFrames(1, () => {
			content.SetAnchorPosX(0f);
			float x = items[selection].RectTransform.anchoredPosition.x;
			if (x > 500f) {
				content.SetAnchorPosX(-(x - 570f + 70f));
			}	
		});
		
		CanvasGroup canvasGroup = scrollView.GetComponent<CanvasGroup>();
		canvasGroup.DOKill();
		canvasGroup.alpha = 0f;
		canvasGroup.DOFade(1f, UIController.defaultTime);

		scrollView.transform.DOKill();
		scrollView.transform.localScale = Vector3.one;
		scrollView.transform.DOPunchScale(Vector3.one * 0.2f, UIController.defaultTime).SetUpdate(true);
	}

	private void HideAnim() {
		CanvasGroup canvasGroup = scrollView.GetComponent<CanvasGroup>();
		canvasGroup.DOKill();
		canvasGroup.DOFade(0f, UIController.defaultTime).OnComplete(() => {
			scrollView.gameObject.SetActive(false);
		}).SetUpdate(true);
		
		scrollView.transform.DOKill();
		scrollView.transform.localScale = Vector3.one;
		scrollView.transform.DOScale(Vector3.one * 0.5f, UIController.defaultTime).SetEase(Ease.InQuad).OnComplete(() => {
			scrollView.transform.localScale = Vector3.one;
		}).SetUpdate(true);
	}

	private void ActivateToggle(int selection) {
		for (int i = 0; i < items.Count; i++) {
			items[i].SetActiveToggle(i == selection);
		}
	}
}
