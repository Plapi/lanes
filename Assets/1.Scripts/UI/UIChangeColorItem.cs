using System;
using UnityEngine;
using UnityEngine.UI;

public class UIChangeColorItem : UIObject {

	[SerializeField] private Button button;
	[SerializeField] private RectTransform content;
	[SerializeField] private Image imageColor;
	[SerializeField] private GameObject toggleObj;

	public void Init(Color color, bool selected, Action onSelected) {
		toggleObj.SetActive(selected);
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(() => {
			onSelected?.Invoke();
		});
		imageColor.color = color;
	}

	public void SetActiveToggle(bool active) {
		toggleObj.SetActive(active);
	}
}
