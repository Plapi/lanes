using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public class UITutorialSpeechBubble : UIObject {

	[SerializeField] private GameObject arrow;
	[SerializeField] private Button audioButton;

	public void Init(UnityAction onAudioButton) {
		audioButton.onClick.AddListener(onAudioButton);
	}

	public void SetAudioButton(bool voiceTutorialDisable) {
		audioButton.transform.GetChild(0).gameObject.SetActive(!voiceTutorialDisable);
		audioButton.transform.GetChild(1).gameObject.SetActive(voiceTutorialDisable);
	}
	
	public void Show() {
		Show(arrow.activeSelf);
	}
	
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
