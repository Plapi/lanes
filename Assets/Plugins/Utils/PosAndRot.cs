using UnityEngine;

public struct PosAndRot {
	public readonly Vector3 position;
	public readonly Quaternion rotation;
	public PosAndRot(Transform transform) {
		position = transform.position;
		rotation = transform.rotation;
	}
}

public static partial class TransformExtensions {
	public static void SetPosAndRot(this Transform transform, PosAndRot posAndRot) {
		transform.position = posAndRot.position;
		transform.rotation = posAndRot.rotation;
	}
}