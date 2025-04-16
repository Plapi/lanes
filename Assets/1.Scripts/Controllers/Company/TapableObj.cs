using System;
using UnityEngine;

public class TapableObj : MonoBehaviour {
	
	[SerializeField] private AudioClip audioClip;

	private Action onTap;
	
	public void SetOnTap(Action onTap) {
		this.onTap = onTap;
	}

	public void OnTap() {
		if (audioClip != null) {
			AudioSystem.Play(audioClip);
#if UNITY_IOS
			HapticFeedback.VibrateHaptic(HapticFeedback.Type.Light);
#endif
		}
		onTap?.Invoke();
	}
}
