using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public static class MonoBehaviourExtensions {
	
	public static Coroutine Wait(this MonoBehaviour behaviour, float delay, Action onComplete) {
		return behaviour != null ? behaviour.StartCoroutine(Wait(delay, onComplete)) : null;
	}

	private static IEnumerator Wait(float delayTime, Action onComplete) {
		if (delayTime <= 0) {
			yield return null;
		} else {
			yield return new WaitForSeconds(delayTime);
		}
		onComplete?.Invoke();
	}
	
	public static void EndOfFrame(this MonoBehaviour behaviour, Action onComplete) {
		if (behaviour != null) {
			behaviour.StartCoroutine(EndOfFrame(onComplete));
		}
	}

	private static IEnumerator EndOfFrame(Action onComplete) {
		yield return new WaitForEndOfFrame();
		onComplete?.Invoke();
	}
	
	public static void WaitForFrames(this MonoBehaviour behaviour, int frames, Action onComplete) {
		behaviour.StartCoroutine(WaitForFrames(frames, onComplete));
	}

	private static IEnumerator WaitForFrames(int frames, Action onComplete) {
		while (frames > 0) {
			frames--;
			yield return null;
		}
		onComplete?.Invoke();
	}
	
	public static void ExecuteForNextFrames(this MonoBehaviour behaviour, int frames, Action action, Action onComplete = null) {
		behaviour.StartCoroutine(ExecuteForNextFrames(frames, action, onComplete));
	}

	private static IEnumerator ExecuteForNextFrames(int frames, Action action, Action onComplete) {
		while (frames > 0) {
			action?.Invoke();
			frames--;
			yield return null;
		}
		onComplete?.Invoke();
	}
	
	public static IEnumerator WaitUntil(Func<bool> predicate, Action onComplete) {
		yield return new WaitUntil(predicate);
		onComplete?.Invoke();
	}

	public static void WaitUntil(this MonoBehaviour behaviour, Func<bool> predicate, Action onSuccess, Action onFail = null, float timeout = 10f) {
		behaviour.StartCoroutine(WaitUntil(predicate, onSuccess, onFail, timeout));
	}

	private static IEnumerator WaitUntil(Func<bool> predicate, Action onSuccess, Action onFail, float timeout) {
		while (true) {
			if (predicate()) {
				onSuccess?.Invoke();
				yield break;
			}

			yield return null;

			timeout -= Time.deltaTime;
			if (timeout < 0f) {
				onFail?.Invoke();
				yield break;
			}
		}
	}
	
	public static Coroutine LoopAction(this MonoBehaviour behaviour, Action action, float time = 1f) {
		if (behaviour != null) {
			return behaviour.StartCoroutine(LoopAction(action, time));
		}
		return null;
	}

	private static IEnumerator LoopAction(Action action, float time) {
		WaitForSeconds waitForSeconds = new WaitForSeconds(time);
		while (true) {
			action?.Invoke();
			yield return waitForSeconds;
		}
	}
	
	public static void ExecuteUntil(this MonoBehaviour behaviour, Func<bool> condition, Action action, Action onComplete = null) {
		if (behaviour != null) {
			behaviour.StartCoroutine(ExecuteUntil(condition, action, onComplete));
		}
	}

	private static IEnumerator ExecuteUntil(Func<bool> condition, Action action, Action onComplete = null) {
		while (!condition()) {
			action();
			yield return null;
		}
		onComplete?.Invoke();
	}
}
