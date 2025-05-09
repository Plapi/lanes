using System;
using System.Collections;
using System.Collections.Generic;
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
	
	public static void ShuffleList<T>(List<T> list) {
		int n = list.Count;
		for (int i = n - 1; i > 0; i--) {
			int j = random.Next(i + 1);
			(list[i], list[j]) = (list[j], list[i]);
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

	public static T SelectRandomItem<T>(T[] items, float[] probabilities, out int index) {
		if (items.Length == 1) {
			return items[index = 0];
		}
		double totalWeight = probabilities.Sum();
		if (totalWeight == 0) {
			throw new InvalidOperationException("All probabilities are zero, no item can be selected.");
		}
		double randomValue = random.NextDouble() * totalWeight;
		double cumulative = 0;
		for (int i = 0; i < items.Length; i++) {
			cumulative += probabilities[i];
			if (randomValue <= cumulative) {
				index = i;
				return items[i];
			}
		}
		index = items.Length - 1;
		return items[^1];
	}

	public static void EnumerateEnum<T>(Action<T> action) {
		foreach (T item in Enum.GetValues(typeof(T))) {
			action?.Invoke(item);
		}
	}

	public static void EnumerateEnum<T>(Action<T, int> action) {
		int index = 0;
		foreach (T item in Enum.GetValues(typeof(T))) {
			action?.Invoke(item, index);
			index++;
		}
	}

	public static bool IsOverUI() {
		if (EventSystem.current == null) {
			return false;
		}
		if (EventSystem.current.IsPointerOverGameObject()) {
			return true;
		}
		return Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId);
	}
	
	public static string EscapeURL(string text) {
		return UnityEngine.Networking.UnityWebRequest.EscapeURL(text).Replace("+", "%20");
	}

	public static bool GetIntersection(Vector3 p0, Vector3 dir0, Vector3 p1, Vector3 dir1, out Vector3 intersection) {
		intersection = Vector2.zero;
		float det = dir0.x * dir1.z - dir0.z * dir1.x;
		if (Mathf.Abs(det) < Mathf.Epsilon) {
			return false;
		}
		Vector3 diff = p1 - p0;
		float t = (diff.x * dir1.z - diff.z * dir1.x) / det;
		intersection = p0 + t * dir0;
		return true;
	}
	
	public static Vector3 GetNearestPoints(Vector3 p0, Vector3 p1, Vector3 target, Vector3 dir, out Vector3 leftPoint, out Vector3 rightPoint) {
		Vector2 point = GetNearestPoints(p0.ToVector2(), p1.ToVector2(), target.ToVector2(), dir.ToVector2(), out Vector2 leftPointV2, out Vector2 rightPointV2);
		leftPoint = leftPointV2.ToVector3();
		rightPoint = rightPointV2.ToVector3();
		return point.ToVector3();
	}
	
	public static Vector2 GetNearestPoints(Vector2 p0, Vector2 p1, Vector2 target, Vector2 dir, out Vector2 leftPoint, out Vector2 rightPoint) {
		Vector2 projP0 = p0 + Vector2.Dot(target - p0, dir) / Vector2.Dot(dir, dir) * dir;
		Vector2 projP1 = p1 + Vector2.Dot(target - p1, dir) / Vector2.Dot(dir, dir) * dir;

		float sign = Vector2.Perpendicular(dir).x * (p1 - p0).y - Vector2.Perpendicular(dir).y * (p1 - p0).x;

		if (sign > 0) {
			leftPoint = projP0;
			rightPoint = projP1;
		} else {
			leftPoint = projP1;
			rightPoint = projP0;
		}
		
		if (Vector2.Dot(target - leftPoint, rightPoint - leftPoint) >= 0 && Vector2.Dot(target - rightPoint, leftPoint - rightPoint) >= 0) {
			return target;
		}
		
		return Vector2.Dot(target - leftPoint, rightPoint - leftPoint) < 0 ? leftPoint : rightPoint;
	}
	
	public static float ComputeProgress(Vector3 targetPos, Vector3 left0, Vector3 right0, Vector3 left1, Vector3 right1) {
		Vector3 mid0 = (left0 + right0) * 0.5f;
		Vector3 mid1 = (left1 + right1) * 0.5f;

		Vector3 dir = mid1 - mid0;
		float length = dir.magnitude;
		dir.Normalize();

		Vector3 toTarget = targetPos - mid0;
		float projection = Vector3.Dot(toTarget, dir);

		return Mathf.Clamp01(projection / length);
	}
	
	public static Vector3 KeepOnSide(Vector3 point, Vector3 direction, Vector3 target) {
		Vector2 dir = new Vector2(direction.x, direction.z).normalized;
		Vector2 perpDir = new Vector2(-dir.y, dir.x);
		Vector2 pointToTarget = new Vector2(target.x - point.x, target.z - point.z);
		float dot = Vector2.Dot(pointToTarget, perpDir);
		if (dot > 0) {
			return target;
		}
		float t = Vector2.Dot(pointToTarget, dir);
		Vector2 closestPoint = new Vector2(point.x, point.z) + t * dir;
		return new Vector3(closestPoint.x, 0, closestPoint.y);
	}
	
	public static Vector3 ClampDirection(Vector3 newDir, Vector3 dirA, Vector3 dirB) {
		// Normalize input directions
		dirA.Normalize();
		dirB.Normalize();
		newDir.Normalize();

		// Check if newDir is inside the angle range
		float dotA = Vector3.Dot(newDir, dirA);
		float dotB = Vector3.Dot(newDir, dirB);

		if (dotA >= 0 && dotB >= 0)
		{
			// It's already inside the range
			return newDir;
		}

		// Find which boundary is closer
		float angleToA = Vector3.Angle(newDir, dirA);
		float angleToB = Vector3.Angle(newDir, dirB);

		return angleToA < angleToB ? dirA : dirB; // Return the closest boundary direction
	}
	
	public static bool CoinFlip() {
		return UnityEngine.Random.Range(0, 2) == 1;
	}

	public static string FormatInt(int value) {
		return value switch {
			>= 1_000_000_000 => (value / 1_000_000_000D).ToString("0.#") + "B",
			>= 1_000_000 => (value / 1_000_000D).ToString("0.#") + "M",
			>= 1_000 => (value / 1_000D).ToString("0.#") + "K",
			_ => value.ToString("N0")
		};
	}
}