using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class UIPanel<T> : UIPanelBase where T: UIPanelBase.Data {

	[SerializeField] protected Image background;
	[SerializeField] protected GameObject content;
	[SerializeField] protected Button[] closeButtons;

	protected T data;
	
	public void Init(T data) {
		this.data = data;

		for (int i = 0; i < closeButtons.Length; i++) {
			closeButtons[i].onClick.RemoveAllListeners();
			closeButtons[i].onClick.AddListener(Hide);
		}
		
		OnInit();
	}

	protected abstract void OnInit();
	
	public void Show() {
		gameObject.SetActive(true);
	}

	public void Hide() {
		gameObject.SetActive(false);
		data.onClose?.Invoke();
	}
}

public abstract class UIPanelBase : MonoBehaviour {
	public abstract class Data {
		public UnityAction onClose;
	}
}
