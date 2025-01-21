using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public abstract class Lane : MonoBehaviour {
	
	[SerializeField] private Element[] elements;
	
	[Space]
	[SerializeField] private List<Element> instantiatedElements;
	[SerializeField] private float currentLength;
	
	public void SetElements(float length) {
		if (elements == null || elements.Length == 0) {
			return;
		}
		
		instantiatedElements ??= new List<Element>();

		float newLength = currentLength;
		while (true) {
			Element randomElement = GetElement();
			newLength += randomElement.Size.z;
			if (newLength > length) {
				break;
			}
			Element element = Instantiate(randomElement, transform);
			element.name = element.name.Replace("(Clone)", "");
			element.transform.SetLocalZ(newLength);
			instantiatedElements.Add(element);
			currentLength = newLength;
		}
	}

	private void ClearElements() {
		if (instantiatedElements == null || instantiatedElements.Count == 0) {
			return;
		}
		foreach (var element in instantiatedElements) {
			if (Application.isPlaying) {
				Destroy(element.gameObject);
			} else {
				DestroyImmediate(element.gameObject);
			}
		}
		instantiatedElements.Clear();
		currentLength = 0;
	}

	private Element GetElement() {
		return elements[Random.Range(0, elements.Length)];
	}
	
#if UNITY_EDITOR
	[SerializeField] [Range(0, 500)] private float debugLength;
	[SerializeField] private bool drawGizmos;
	private float prevDebugLength;
	protected virtual void OnDrawGizmos() {
		if (!drawGizmos) {
			return;
		}
		Gizmos.color = Color.red;
		Gizmos.DrawCube(transform.position, Vector3.one * 0.2f);
		Vector3 endPoint = transform.position + transform.forward * debugLength;
		Gizmos.color = Color.green;
		Gizmos.DrawCube(endPoint, Vector3.one * 0.2f);
		if (Mathf.Abs(prevDebugLength - debugLength) > Mathf.Epsilon) {
			if (debugLength > 0) {
				SetElements(debugLength);
			} else {
				ClearElements();
			}
			prevDebugLength = debugLength;
		}
	}
#endif

}
