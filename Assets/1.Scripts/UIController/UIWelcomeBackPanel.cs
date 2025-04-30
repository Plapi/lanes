using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIWelcomeBackPanel : UIPanel<UIWelcomeBackPanel.Data> {

	[SerializeField] private TextMeshProUGUI descriptionText;
	[SerializeField] private TextMeshProUGUI incomeText;
	[SerializeField] private Button watchAdButton;
	[SerializeField] private Button continueButton;
	
	[SerializeField] private AudioClip showAudioClip;
	
	protected override void OnInit() {

		string timeString = data.seconds < 3600 ? $"{data.seconds / 60}m" : $"{data.seconds / 3600}h";
		descriptionText.text = $"You where offline for <color=#FFD60A>{timeString}.</color>\nWhile you were away, you <color=#FFD60A>earned:</color>";
		
		incomeText.text = $"+{data.income:N0}";
		
		watchAdButton.onClick.RemoveAllListeners();
		watchAdButton.onClick.AddListener(() => {
			Close();
			data.onWatchAd?.Invoke();
		});
		
		continueButton.onClick.RemoveAllListeners();
		continueButton.onClick.AddListener(() => {
			Close();
			data.onContinue?.Invoke();
		});
	}

	protected override void ShowAnim(Action onComplete) {
		base.ShowAnim(onComplete);
		AudioSystem.Play(showAudioClip);
	}

	public new class Data : UIPanelBase.Data {
		public int seconds;
		public int income;
		public Action onWatchAd;
		public Action onContinue;
	}
	
}
