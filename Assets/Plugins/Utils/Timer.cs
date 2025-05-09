using UnityEngine;
using System;

public class Timer : MonoBehaviour {

	private float currentTime;
	private float totalTime;
	private Action onUpdate;

	public static Timer Create(GameObject obj, float time, Action onUpdate) {
		return obj.AddComponent<Timer>().Init(time, onUpdate);
	}

	private Timer Init(float time, Action onUpdate) {
		currentTime = totalTime = time;
		this.onUpdate = onUpdate;
		this.onUpdate();
		return this;
	}

	private void Update() {
		currentTime += Time.deltaTime;
		if (currentTime >= totalTime) {
			onUpdate();
			currentTime = totalTime;
		}
	}
}
