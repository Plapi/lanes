#if UNITY_EDITOR
using UnityEngine;

public class Tree : MonoBehaviour {

	[SerializeField] private float radius;
	[SerializeField] private bool drawGizmos = true;

	public float Radius => radius;

	private void OnDrawGizmos() {
		if (drawGizmos) {
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, radius);
		}
	}
}
#endif
