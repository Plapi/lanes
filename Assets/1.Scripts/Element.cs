using UnityEngine;

public abstract class Element : MonoBehaviour {
	
	[SerializeField] private MeshRenderer meshRenderer;
	[SerializeField] private Vector3 size;
	
	public Vector3 Size => size;
	
#if UNITY_EDITOR
	[ContextMenu("Set Size")]
	private void SetSize() {
		if (meshRenderer == null) {
			return;
		}
		size = meshRenderer.bounds.size;
	}
#endif
}
