using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Newtonsoft.Json;

public class UIWatchAdPanel : UIPanel<UIWatchAdPanel.Data> {

	[SerializeField] private Slider slider;
	[SerializeField] private TextMeshProUGUI timeText;
	[SerializeField] private TextMeshProUGUI endTimeText;
	[SerializeField] private Button watchButton;

	[Space]
	[SerializeField] private Image sliderFill;
	[SerializeField] private Sprite activeSlideSprite;
	[SerializeField] private Sprite inactiveSlideSprite;
	
	protected override void OnInit() {

		StartCoroutine(UpdateSlider());
		
		watchButton.onClick.RemoveAllListeners();
		watchButton.onClick.AddListener(() => {
			AdsController.Instance.ShowAd(success => {
				if (success) {
					const int minutes = 15;
					DateTime now = DateTime.Now;
					WatchAdBoostIncome watchAdBoostIncome = PlayerPrefsManager.UserData.watchAdBoostIncome;
					if (watchAdBoostIncome != null && watchAdBoostIncome.endTime.Date > now) {
						watchAdBoostIncome.endTime.Date = watchAdBoostIncome.endTime.Date.AddMinutes(minutes);
					} else {
						watchAdBoostIncome = new WatchAdBoostIncome {
							startTime = new SerializedDateTime(now),
							endTime = new SerializedDateTime(now.AddMinutes(minutes))
						};
					}
					PlayerPrefsManager.UserData.watchAdBoostIncome = watchAdBoostIncome;
					PlayerPrefsManager.SaveUserData();
				}
			});
		});
	}

	private IEnumerator UpdateSlider() {
		WaitForSeconds wait = new WaitForSeconds(1f);
		while (true) {
			WatchAdBoostIncome watchAdBoostIncome = PlayerPrefsManager.UserData.watchAdBoostIncome;
			DateTime now = DateTime.Now;
			if (watchAdBoostIncome != null && watchAdBoostIncome.endTime.Date > now) {
			
				int totalMinutes = Mathf.RoundToInt((float)(watchAdBoostIncome.endTime.Date - watchAdBoostIncome.startTime.Date).TotalMinutes);
				endTimeText.text = $"{totalMinutes}m";
				
				int currentMinutes = Mathf.RoundToInt((float)(now - watchAdBoostIncome.startTime.Date).TotalMinutes);
				slider.value = (float)currentMinutes / totalMinutes;
				
				TimeSpan timeSpan = watchAdBoostIncome.endTime.Date - now;
				timeText.text = timeSpan.Hours > 1 ? $"{timeSpan.Hours}h {timeSpan.Minutes}m" :
					$"{timeSpan.Minutes}m {timeSpan.Seconds}s";
				
				sliderFill.sprite = activeSlideSprite;
			} else {
				slider.value = 1f;
				timeText.text = "0m 0s";
				sliderFill.sprite = inactiveSlideSprite;
			}
			
			yield return wait;
		}
	}
	
	protected override void ShowAnim(Action onComplete) {
		gameObject.SetActive(true);
		RectTransform contentRect = content.GetComponent<RectTransform>();
		contentRect.SetAnchorPosY(-800f);
		contentRect.DOAnchorPosY(-70f, UIController.defaultTime).SetEase(Ease.OutQuad).OnComplete(() => onComplete());
	}

	protected override void CloseAnim(bool anim, Action onComplete) {
		RectTransform contentRect = content.GetComponent<RectTransform>();
		contentRect.DOAnchorPosY(-800f, UIController.defaultTime).SetEase(Ease.InQuad).OnComplete(() => {
			gameObject.SetActive(false);
		});
		onComplete();
	}

	public new class Data : UIPanelBase.Data {
		
	}
}

[Serializable]
public class WatchAdBoostIncome {
	public SerializedDateTime startTime;
	public SerializedDateTime endTime;
}
