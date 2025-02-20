using UnityEngine;
using UnityEngine.Rendering;

public class Building : Element {

	[SerializeField] private int length;

	public int Length => length;
	
	private void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Gizmos.DrawLine(transform.position, transform.position - transform.forward * length);
	}
	
#if UNITY_EDITOR
	[SerializeField] private bool castShadows;
	[ContextMenu("Set Cast Shadows")]
	private void SetCastShadows() {
		MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < meshRenderers.Length; i++) {
			meshRenderers[i].shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
		}
	}
#endif
}
