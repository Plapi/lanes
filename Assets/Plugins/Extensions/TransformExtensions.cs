using System;
using UnityEngine;

public static partial class TransformExtensions {
	
	public static void SetX(this Transform transform, float x) {
		transform.position = new Vector3(x, transform.position.y, transform.position.z);
	}
	
	public static void SetY(this Transform transform, float y) {
		transform.position = new Vector3(transform.position.x, y, transform.position.z);
	}
	
	public static void SetZ(this Transform transform, float z) {
		transform.position = new Vector3(transform.position.x, transform.position.y, z);
	}
	
	public static void SetXY(this Transform transform, float x, float y) {
		transform.position = new Vector3(x, y, transform.position.z);
	}
	
	public static void SetXZ(this Transform transform, float x, float z) {
		transform.position = new Vector3(x, transform.position.y, z);
	}
	
	public static void SetYZ(this Transform transform, float y, float z) {
		transform.position = new Vector3(transform.position.z, y, z);
	}
	
	public static void SetXYZ(this Transform transform, float x, float y, float z) {
		transform.position = new Vector3(x, y, z);
	}
	
	public static void SetLocalX(this Transform transform, float x) {
		transform.localPosition = new Vector3(x, transform.localPosition.y, transform.localPosition.z);
	}
	
	public static void SetLocalY(this Transform transform, float y) {
		transform.localPosition = new Vector3(transform.localPosition.x, y, transform.localPosition.z);
	}
	
	public static void SetLocalZ(this Transform transform, float z) {
		transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, z);
	}
	
	public static void SetLocalXY(this Transform transform, float x, float y) {
		transform.localPosition = new Vector3(x, y, transform.localPosition.z);
	}
	
	public static void SetLocalXZ(this Transform transform, float x, float z) {
		transform.localPosition = new Vector3(x, transform.localPosition.y, z);
	}
	
	public static void SetLocalYZ(this Transform transform, float y, float z) {
		transform.localPosition = new Vector3(transform.localPosition.x, y, z);
	}
	
	public static void SetLocalXYZ(this Transform transform, float x, float y, float z) {
		transform.localPosition = new Vector3(x, y, z);
	}
	
	public static void SetScaleX(this Transform transform, float x) {
		transform.localScale = new Vector3(x, transform.localScale.y, transform.localScale.z);
	}
	
	public static void SetScaleY(this Transform transform, float y) {
		transform.localScale = new Vector3(transform.localScale.x, y, transform.localScale.z);
	}
	
	public static void SetScaleZ(this Transform transform, float z) {
		transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, z);
	}
	
	public static void SetScale(this Transform transform, float scale) {
		transform.localScale = Vector3.one * scale;
	}
	
	public static void SetAngleX(this Transform transform, float x) {
		transform.eulerAngles = new Vector3(x, transform.eulerAngles.y, transform.eulerAngles.z);
	}
	
	public static void SetAngleY(this Transform transform, float y) {
		transform.eulerAngles = new Vector3(transform.eulerAngles.x, y, transform.eulerAngles.z);
	}
	
	public static void SetAngleZ(this Transform transform, float z) {
		transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, z);
	}

	public static void SetLocalAngleX(this Transform transform, float x) {
		transform.localEulerAngles = new Vector3(x, transform.localEulerAngles.y, transform.localEulerAngles.z);
	}
	
	public static void SetLocalAngleY(this Transform transform, float y) {
		transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, y, transform.localEulerAngles.z);
	}
	
	public static void SetLocalAngleZ(this Transform transform, float z) {
		transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, z);
	}
	
	public static void SetLocalAngleXY(this Transform transform, float x, float y) {
		transform.localEulerAngles = new Vector3(x, y, transform.localEulerAngles.z);
	}
	
	public static void SetLocalAngleXZ(this Transform transform, float x, float z) {
		transform.localEulerAngles = new Vector3(x, transform.localEulerAngles.y, z);
	}
	
	public static void SetLocalAngleYZ(this Transform transform, float y, float z) {
		transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, y, z);
	}
	
	public static void IterateAllChildren(Transform parent, Action<Transform> action) {
		action(parent);
		foreach (Transform child in parent) {
			IterateAllChildren(child, action);
		}
	}
	
	public static bool TryGetChild(this Transform transform, int index, out Transform child) {
		child = null;
		if (transform.childCount > index) {
			child = transform.GetChild(index);
			return true;
		}
		return false;
	}
	
	public static void HideAllChildrenExcept(this Transform transform, int index) {
		foreach (Transform t in transform) {
			t.gameObject.SetActive(false);
		}
		if (transform.TryGetChild(index, out Transform child)) {
			child.gameObject.SetActive(true);
		} else {
			Debug.LogError($"Child index {index} not found");
		}
	}
	
	public static Vector3 ToVector3(this Vector2 pos) {
		return new Vector3(pos.x, 0f, pos.y);
	}
	
	public static Vector2 ToVector2(this Vector3 pos) {
		return new Vector2(pos.x, pos.z);
	}
}
