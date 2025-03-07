using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIButtonSound : MonoBehaviour {
	
	[SerializeField] private AudioClip audioClip;
	
	public void PlaySound() {
		this.PlaySound(gameObject, audioClip);
	}

#if UNITY_EDITOR
	public void AddButtonEvent(Button button, AudioClip audioClip) {
		this.audioClip = audioClip;
		
		UnityEvent onClickEvent = button.onClick;
		bool alreadyRegistered = false;
		
		for (int i = 0; i < onClickEvent.GetPersistentEventCount(); i++) {
			if (onClickEvent.GetPersistentTarget(i) == this && onClickEvent.GetPersistentMethodName(i) == "PlaySound") {
				alreadyRegistered = true;
				break;
			}
		}
			
		if (!alreadyRegistered) {
			UnityAction action = PlaySound;
			UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, action);
			EditorUtility.SetDirty(button);
		}
	}
#endif
	
}