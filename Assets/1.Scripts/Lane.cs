using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public abstract class Lane : MonoBehaviour {
	
	[SerializeField] private Element[] elements;
	
	[Space]
	[SerializeField] private List<Element> instantiatedElements;

	public float Length { get; private set; }

	public void SetElements(int length) {
		if (elements == null || elements.Length == 0) {
			return;
		}

		for (int i = 0; i < elements.Length; i++) {
			elements[i].Id = elements[i].GetInstanceID().ToString();
			ObjectPoolManager.CreatePool(elements[i]);
		}
		ClearElements();
		
		instantiatedElements ??= new List<Element>();

		int elementsCount = length / Settings.Instance.laneSize;
		for (int i = 0; i < elementsCount; i++) {
			Element element = ObjectPoolManager.Get(GetElement(), transform);
			element.name = element.name.Replace("(Clone)", "");
			element.transform.SetLocalZ(i * Settings.Instance.laneSize);
			instantiatedElements.Add(element);
		}
		Length = length;
	}

	public void ClearElements() {
		if (instantiatedElements == null || instantiatedElements.Count == 0) {
			return;
		}
		foreach (var element in instantiatedElements) {
			ObjectPoolManager.Release(element);
		}
		instantiatedElements.Clear();
	}

	private Element GetElement() {
		return elements[Random.Range(0, elements.Length)];
	}
	
#if UNITY_EDITOR
	[Space]
	[SerializeField] [Range(0, 500)] private int debugLength = 500;
	[ContextMenu("Set Debug Elements")]
	private void SetDebugElements() {
		SetElements(debugLength);
	}
#endif

}
