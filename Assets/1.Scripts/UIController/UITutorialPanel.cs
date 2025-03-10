using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using DG.Tweening;

public class UITutorialPanel : UIPanel<UITutorialPanel.Data> {

	[SerializeField] private VerticalLayoutGroup group;
	[SerializeField] private RectTransform speechBubble;
	[SerializeField] private TextMeshProUGUI bubbleText;
	[SerializeField] private Button nextButton;

	[Space]
	[SerializeField] private GameObject verticalAnim;
	[SerializeField] private GameObject horizontalAnim;

	[Space]
	[SerializeField] private AudioClip showBubbleAudio;
	
	protected override void OnInit() {
		speechBubble.gameObject.SetActive(false);
		nextButton.gameObject.SetActive(false);
		verticalAnim.SetActive(false);
	}

	public void ShowText(string text) {

		bubbleText.text = text;

		CanvasGroup canvasGroup = speechBubble.GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
		speechBubble.gameObject.SetActive(true);
		canvasGroup.DOFade(1f, UIController.defaultTime);
		speechBubble.DOPunchScale(Vector3.one * 0.2f, UIController.defaultTime);

		this.EndOfFrame(() => {
			group.enabled = false;
			group.enabled = true;	
		});
		
		AudioSystem.Play(showBubbleAudio);
#if UNITY_IOS
		HapticFeedback.VibrateHaptic(HapticFeedback.Type.Light);
#endif
		
	}

	public void HideText() {
		CanvasGroup canvasGroup = speechBubble.GetComponent<CanvasGroup>();
		canvasGroup.DOFade(0f, UIController.defaultTime);
		speechBubble.DOScale(Vector3.one * 0.5f, UIController.defaultTime).SetEase(Ease.InQuad).OnComplete(() => {
			speechBubble.localScale = Vector3.one;
			speechBubble.gameObject.SetActive(false);
		}).SetUpdate(true);
	}

	public void ShowNextButton(UnityAction onNext) {
		CanvasGroup canvasGroup = nextButton.GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
		nextButton.gameObject.SetActive(true);
		canvasGroup.DOFade(1f, UIController.defaultTime);
		nextButton.onClick.RemoveAllListeners();
		nextButton.onClick.AddListener(() => {
			nextButton.onClick.RemoveAllListeners();
			onNext?.Invoke();
		});
	}

	public void HideNextButton() {
		CanvasGroup canvasGroup = nextButton.GetComponent<CanvasGroup>();
		canvasGroup.DOFade(0f, UIController.defaultTime).OnComplete(() => {
			nextButton.gameObject.SetActive(false);
		});
	}

	public void ShowVerticalAnim() {
		ShowAnim(verticalAnim);
	}

	public void ShowHorizontalAnim() {
		ShowAnim(horizontalAnim);
	}

	private static void ShowAnim(GameObject anim) {
		CanvasGroup canvasGroup = anim.GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
		anim.SetActive(true);
		canvasGroup.DOFade(1f, UIController.defaultTime);
		
		EventTrigger eventTrigger = anim.GetComponent<EventTrigger>();
		
		EventTrigger.Entry enterEntry = new EventTrigger.Entry {
			eventID = EventTriggerType.BeginDrag,
		};
		enterEntry.callback.AddListener(_ => {
			anim.transform.GetChild(0).gameObject.SetActive(false);
		});
		
		EventTrigger.Entry exitEntry = new EventTrigger.Entry {
			eventID = EventTriggerType.EndDrag,
		};
		exitEntry.callback.AddListener(_ => {
			anim.transform.GetChild(0).gameObject.SetActive(true);
		});
		
		eventTrigger.triggers = new List<EventTrigger.Entry> { enterEntry, exitEntry };
	}

	public void HideVerticalAnim() {
		verticalAnim.gameObject.SetActive(false);
	}
	
	public void HideHorizontalAnim() {
		horizontalAnim.gameObject.SetActive(false);
	}

	public new class Data: UIPanelBase.Data {
		
	}
}