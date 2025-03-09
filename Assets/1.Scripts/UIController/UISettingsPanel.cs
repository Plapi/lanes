using System;
using UnityEngine;
using UnityEngine.UI;

public class UISettingsPanel : UIPanel<UISettingsPanel.Data> {

	[SerializeField] private Slider[] sliders;
	
	protected override void OnInit() {

		for (int i = 0; i < sliders.Length; i++) {
			int index = i;
			sliders[i].value = data.volumes[index];
			sliders[i].onValueChanged.RemoveAllListeners();
			sliders[i].onValueChanged.AddListener(value => {
				data.onUpdateSlider(index, value);
			});
		}
		
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
		public Action<int, float> onUpdateSlider;
		public new Action<float[]> onClose;
	}
	
}