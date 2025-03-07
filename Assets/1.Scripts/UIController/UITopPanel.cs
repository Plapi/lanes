using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class UITopPanel : UIPanel<UITopPanel.Data> {

	[Space]
	[SerializeField] private Button pauseButton;
	
	[Space]
	[SerializeField] private Slider healthSlider;
	[SerializeField] private Image healthFillGreen;
	[SerializeField] private Image healthFillRed;
	
	[Space]
	[SerializeField] private RectTransform personTransform;
	[SerializeField] private Image personIcon;
	[SerializeField] private RectTransform personSpeechBubble;
	[SerializeField] private TextMeshProUGUI personSpeechBubbleText;
	[SerializeField] private Color personSpeechBubbleTextNormalColor;
	[SerializeField] private Color personSpeechBubbleTextFailColor;
	[SerializeField] private UICoinsAnim coinsAnim;

	[Space]
	[SerializeField] private RectTransform distanceTransform;
	[SerializeField] private TextMeshProUGUI distanceText;
	
	private Coroutine waitCoroutine;
	
	protected override void OnInit() {
		pauseButton.onClick.AddListener(data.onPause);
	}

	public void ResetItems() {
		DOTween.Kill(healthSlider);
		DOTween.Kill(healthSlider.transform);
		healthSlider.transform.localScale = Vector3.one;
		healthSlider.value = 1f;
		healthFillGreen.SetAlpha(1f);
		healthFillRed.SetAlpha(0f);
		
		if (waitCoroutine != null) {
			StopCoroutine(waitCoroutine);
			waitCoroutine = null;
		}
		personSpeechBubble.gameObject.SetActive(false);
		personTransform.gameObject.SetActive(false);
		personSpeechBubble.DOKill();
		personSpeechBubble.GetComponent<CanvasGroup>().DOKill();
	}

	public void UpdateHealthSlider(float value) {
		DOTween.Kill(healthSlider);
		DOTween.Kill(healthSlider.transform);
		healthSlider.transform.localScale = Vector3.one;
		healthSlider.DOValue(value, UIController.defaultTime).SetEase(Ease.OutCubic).OnUpdate(() => {
			healthFillGreen.SetAlpha(healthSlider.value);
			healthFillRed.SetAlpha(1f - healthSlider.value);
		});
		healthSlider.transform.DOPunchScale(Vector3.one * 0.2f, UIController.defaultTime).SetUpdate(true);
	}

	public void ShowPerson(int blocks) {
		
		if (waitCoroutine != null) {
			StopCoroutine(waitCoroutine);
			waitCoroutine = null;
		}
		
		personSpeechBubble.gameObject.SetActive(false);
		personTransform.gameObject.SetActive(true);
		
		personIcon.SetAlpha(0f);
		personIcon.DOFade(1, 0.2f);
		
		float initY = personTransform.anchoredPosition.y;
		personTransform.SetAnchorPosY(-UIController.Instance.Size.y * 0.75f);
		personTransform.DOAnchorPosY(initY, 0.5f).SetEase(Ease.OutQuad);
		personTransform.DOBlendableRotateBy(new Vector3(0f, 0f, 360f), 0.5f, RotateMode.LocalAxisAdd).OnComplete(() => {
			ShowBubbleSpeech(blocks == 1 ? "Let me off\nnext block!" : $"Let me off\nin {blocks} blocks!",
				personSpeechBubbleTextNormalColor);
		});
	}

	public void HidePerson(int coins) {
		
		if (waitCoroutine != null) {
			StopCoroutine(waitCoroutine);
			waitCoroutine = null;
		}
		
		if (coins > 0) {
			string successText = RandomTextsSystem.Get(RandomTextsSystem.SuccessPerson);
			successText = successText.Replace("#coins#", $"<color=#00D740>{coins}</color>");
			ShowBubbleSpeech(successText, personSpeechBubbleTextNormalColor, -1f);
			coinsAnim.Play(Mathf.Clamp(coins / 20, 3, 15), () => {
				personSpeechBubble.GetComponent<CanvasGroup>().DOFade(0f, 0.2f);
				personIcon.DOFade(0f, 0.2f);
			});
		} else {
			ShowBubbleSpeech(RandomTextsSystem.Get(RandomTextsSystem.FailPerson), personSpeechBubbleTextFailColor, 2f, () => {
				personIcon.DOFade(0f, 0.2f);
			});
		}
		
		HideDistance();
	}

	public void ShowDistance(int distance) {
		if (!distanceTransform.gameObject.activeSelf) {
			distanceTransform.gameObject.SetActive(true);
			distanceTransform.SetAnchorPosY(-70f);
			distanceTransform.DOAnchorPosY(-102f, 0.2f);
		}
		distanceText.text = $"{distance}m";
	}

	public void HideDistance() {
		if (!gameObject.activeSelf) {
			distanceTransform.gameObject.SetActive(false);
			return;
		}
		if (distanceTransform.gameObject.activeSelf) {
			distanceTransform.DOAnchorPosY(-70f, 0.2f).OnComplete(() => {
				distanceTransform.gameObject.SetActive(false);
			});
		}
	}

	private void ShowBubbleSpeech(string text, Color color, float hideDelay = 3f, Action onComplete = null) {
		personSpeechBubbleText.text = text;
		personSpeechBubbleText.color = color;
		personSpeechBubble.gameObject.SetActive(true);
		personSpeechBubble.DOKill();
		personSpeechBubble.transform.localScale = Vector3.one;
		personSpeechBubble.DOPunchScale(Vector3.one * 0.2f, 0.2f).SetUpdate(true);
		CanvasGroup canvasGroup = personSpeechBubble.GetComponent<CanvasGroup>();
		canvasGroup.DOKill();
		canvasGroup.alpha = 0f;
		canvasGroup.DOFade(1f, 0.2f);
		if (hideDelay > 0f) {
			waitCoroutine = this.Wait(hideDelay, () => {
				canvasGroup.DOFade(0f, 0.2f);
				onComplete?.Invoke();
			});	
		}
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
