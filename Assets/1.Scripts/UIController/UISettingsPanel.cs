using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISettingsPanel : UIPanel<UISettingsPanel.Data> {

	[SerializeField] private Slider[] sliders;
	[SerializeField] private Slider hapticSlider;
	
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
	}
	
	public new class Data: UIPanelBase.Data {
		public float[] volumes;
		public bool hapticFeedback;
		public Action<int, float> onUpdateSlider;
		public Action<bool> onUpdateHapticFeedback;
		public new Action<float[]> onClose;
	}
	
}