using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = System.Random;

public static class Utils {

	private static readonly Random random = new();

	public static void ShuffleArray<T>(T[] array) {
		int n = array.Length;
		for (int i = n - 1; i > 0; i--) {
			int j = random.Next(i + 1);
			(array[i], array[j]) = (array[j], array[i]);
		}
	}
	
	public static IEnumerator WaitForRealTime(float delay) {
		while (true) {
			float pauseEndTime = Time.realtimeSinceStartup + delay;
			while (Time.realtimeSinceStartup < pauseEndTime) {
				yield return 0;
			}
			break;
		}
	}

	public static T SelectRandomItem<T>(T[] items, float[] probabilities) {
		double totalWeight = probabilities.Sum();
		if (totalWeight == 0) {
			throw new InvalidOperationException("All probabilities are zero, no item can be selected.");
		}
		double randomValue = random.NextDouble() * totalWeight;
		double cumulative = 0;
		for (int i = 0; i < items.Length; i++) {
			cumulative += probabilities[i];
			if (randomValue <= cumulative) {
				return items[i];
			}
		}
		return items[^1];
	}
	
	public static bool IsOverUI() {
		if (EventSystem.current.IsPointerOverGameObject()) {
			return true;
		}
		return Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId);
	}
}