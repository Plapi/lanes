using System;
using System.Collections.Generic;

public class UIController : MonoBehaviourSingleton<UIController> {

	public UIPanelBase[] panels;

	private readonly Dictionary<Type, UIPanelBase> dictPanels = new();

	protected override void Awake() {
		base.Awake();
		for (int i = 0; i < panels.Length; i++) {
			dictPanels.Add(panels[i].GetType(), panels[i]);
		}
	}

	public T GetPanel<T>() where T : UIPanelBase {
		return dictPanels[typeof(T)] as T;
	}
}
