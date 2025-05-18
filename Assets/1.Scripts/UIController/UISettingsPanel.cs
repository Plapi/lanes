using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UISettingsPanel : UIPanel<UISettingsPanel.Data> {

	[SerializeField] private Slider[] sliders;
	[SerializeField] private Slider hapticSlider;
	[SerializeField] private Button aboutButton;
	
	protected override void OnInit() {

		for (int i = 0; i < sliders.Length; i++) {
			int index = i;
			sliders[i].value = data.volumes[index];
			sliders[i].onValueChanged.RemoveAllListeners();
			sliders[i].onValueChanged.AddListener(value => {
				data.onUpdateSlider(index, value);
			});
		}
		
		hapticSlider.value = data.hapticFeedback ? 1 : 0;
		EventTrigger.Entry endDragEntry = new EventTrigger.Entry {
			eventID = EventTriggerType.EndDrag
		};
		endDragEntry.callback.AddListener(_ => {
			bool hapticFeedback = hapticSlider.value >= 0.5f;
			hapticSlider.value = hapticFeedback ? 1f : 0f;
			data.onUpdateHapticFeedback(hapticFeedback);
		});
		EventTrigger eventTrigger = hapticSlider.GetComponent<EventTrigger>();
		eventTrigger.triggers = new List<EventTrigger.Entry> { endDragEntry };
		
		((UIPanelBase.Data)data).onClose += () => {
			float[] volumes = new float[sliders.Length];
			for (int i = 0; i < sliders.Length; i++) {
				volumes[i] = sliders[i].value;
			}
			data.onClose(volumes);
		};
		
		aboutButton.onClick.AddListener(data.onAbout);
	}
	
	protected override void ShowAnim(Action onComplete) {
		gameObject.SetActive(true);
		RectTransform contentRect = content.GetComponent<RectTransform>();
		contentRect.SetAnchorPosY(-900f);
		contentRect.DOAnchorPosY(-70f, UIController.defaultTime).SetEase(Ease.OutQuad).OnComplete(() => {
			onComplete();
		}).SetUpdate(true);
	}

	protected override void CloseAnim(bool anim, Action onComplete) {
		RectTransform contentRect = content.GetComponent<RectTransform>();
		contentRect.DOAnchorPosY(-900f, UIController.defaultTime).SetEase(Ease.InQuad).OnComplete(() => {
			gameObject.SetActive(false);
		}).SetUpdate(true);
		onComplete();
	}
	
	public new class Data: UIPanelBase.Data {
		public float[] volumes;
		public bool hapticFeedback;
		public Action<int, float> onUpdateSlider;
		public Action<bool> onUpdateHapticFeedback;
		public new Action<float[]> onClose;
		public UnityAction onAbout;
	}
	
}