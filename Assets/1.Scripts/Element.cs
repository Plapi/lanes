using UnityEngine;

public abstract class Element : MonoBehaviour, IPoolableObject<Element> {
	
	[SerializeField] private MeshRenderer meshRenderer;
	[SerializeField] private Vector3 size;

	public Vector3 Size => size;
	
	public string Id { get; set; }
	
	public string GetID() {
		return Id;
	}

	public Element GetMonoBehaviour() {
		return this;
	}
	
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
