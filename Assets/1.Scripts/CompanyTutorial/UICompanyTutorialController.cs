using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UICompanyTutorialController : UIObject {

	[Space]
	[SerializeField] private GameObject tutorialCharacter;
	
	[SerializeField] private Button button;
	[SerializeField] private RectTransform characterRect;
	[SerializeField] private UITutorialSpeechBubble[] speechBubbles;
	[SerializeField] private TutorialStep[] tutorialSteps;
	[SerializeField] private RectTransform tutorialObj;
	[SerializeField] private AudioClip tutorialCompleteSound;
	
	[Space]
	[SerializeField] private AudioSource audioSource;
	
	private int tutorialIndex;
	private Action<TutorialStep> onTutorialStep;
	private Action onTutorialComplete;
	
	public void Init(Action<TutorialStep> onTutorialStep, Action onTutorialComplete) {
		this.onTutorialStep = onTutorialStep;
		this.onTutorialComplete = onTutorialComplete;
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(ShowNextSpeechBubble);
		for (int i = 0; i < tutorialSteps.Length; i++) {
			tutorialSteps[i].index = i;
		}
		gameObject.SetActive(true);
		tutorialCharacter.SetActive(true);
		
		AnalyticsSystem.RecordCompanyTutorialEvent(0);
		
		characterRect.gameObject.SetActive(false);
		speechBubbles[0].gameObject.SetActive(false);
		StartCoroutine(PlayInitShowAnim());

		for (int i = 0; i < speechBubbles.Length; i++) {
			speechBubbles[i].Init(() => {
				PlayerPrefsManager.UserData.voiceTutorialDisable = !PlayerPrefsManager.UserData.voiceTutorialDisable;
				if (PlayerPrefsManager.UserData.voiceTutorialDisable) {
					audioSource.Pause();
				} else {
					audioSource.Play();
				}
				speechBubbles[tutorialIndex].SetAudioButton(PlayerPrefsManager.UserData.voiceTutorialDisable);
			});
		}
	}

	private IEnumerator PlayInitShowAnim() {
		yield return new WaitForSeconds(1f);
		characterRect.gameObject.SetActive(true);
		characterRect.SetAnchorPosX(-340f);
		characterRect.DOAnchorPosX(-65f, 0.25f).SetEase(Ease.OutExpo);
		yield return new WaitForSeconds(0.5f);
		speechBubbles[0].Show();
		PlayCurrentAudioVoice();
	}

	private void ShowNextSpeechBubble() {
		
		button.interactable = false;
		
		TutorialStep step = tutorialSteps[tutorialIndex];
		speechBubbles[tutorialIndex].Hide();
		if (step.tutorialObj != null) {
			step.tutorialObj.gameObject.SetActive(false);
		}
		tutorialIndex++;
		
		if (tutorialIndex >= speechBubbles.Length) {
			AudioSystem.Play(tutorialCompleteSound);
			this.Wait(0.3f, () => {
				tutorialCharacter.SetActive(false);
				onTutorialComplete?.Invoke();
			});	
			return;
		}
		AnalyticsSystem.RecordCompanyTutorialEvent(tutorialIndex);
		
		step = tutorialSteps[tutorialIndex];
		
		speechBubbles[tutorialIndex].Show(step.arrowIsActive);
		speechBubbles[tutorialIndex].SetAudioButton(PlayerPrefsManager.UserData.voiceTutorialDisable);
		PlayCurrentAudioVoice();
		tutorialObj.SetAnchorPosY(step.anchorPosY);
		onTutorialStep?.Invoke(step);

		if (step.tutorialObj != null) {
			this.Wait(0.2f, () => step.tutorialObj.SetActive(true));
			if (step.tutorialButton != null) {
				step.tutorialButton.onClick.AddListener(ShowNextSpeechBubble);
			}
		} else {
			this.Wait(0.3f, () => button.interactable = true);	
		}
	}
	
	private void PlayCurrentAudioVoice() {
		audioSource.clip = Resources.Load<AudioClip>($"Voices/CompanyTutorial/{tutorialIndex}");
		if (!PlayerPrefsManager.UserData.voiceTutorialDisable) {
			audioSource.Play();	
		}
	}
}

[Serializable]
public class TutorialStep {
	[HideInInspector] public int index;
	public Vector3 cameraZoom;
	public float anchorPosY;
	public bool arrowIsActive;
	public GameObject tutorialObj;
	public Button tutorialButton;
}
