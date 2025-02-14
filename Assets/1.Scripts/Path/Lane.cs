using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class Lane<T> : LaneBase where T : LaneData {
	
	public T Data {get; private set;}

	public override void Init(LaneData data) {
		base.Init(data);
		Data = (T)data;
		OnInit();
	}
	
	public int Length => Data.length;

	protected abstract void OnInit();
}

[ExecuteInEditMode]
public abstract class LaneBase : MonoBehaviour {
	
	[SerializeField] private Element[] elements;
	
	private readonly List<Element> instantiatedElements = new();

	public virtual void Init(LaneData data) {
		for (int i = 0; i < elements.Length; i++) {
			elements[i].Id = elements[i].GetInstanceID().ToString();
			ObjectPoolManager.CreatePool(elements[i]);
		}
		int elementsCount = data.length / Settings.Instance.laneSize;
		for (int i = 0; i < elementsCount; i++) {
			Element element = ObjectPoolManager.Get(GetElement(), transform);
			element.name = element.name.Replace("(Clone)", "");
			element.transform.SetLocalZ(i * Settings.Instance.laneSize);
			element.gameObject.SetActive(true);
			instantiatedElements.Add(element);
		}
	}
	public virtual void Clear() {
		foreach (var element in instantiatedElements) {
			ObjectPoolManager.Release(element);
		}
		instantiatedElements.Clear();
	}

	private Element GetElement() {
		return elements[Random.Range(0, elements.Length)];
	}
}

public class LaneData {
	public LaneType type;
	public int length;
}

public enum LaneType {
	RoadLaneSingleLeft,
	RoadLaneSingleRight,
	RoadLaneMiddle,
	RoadLaneEdgeLeft,
	RoadLaneEdgeRight,
	SideWalkLaneRight,
	SideWalkLaneLeft
}
