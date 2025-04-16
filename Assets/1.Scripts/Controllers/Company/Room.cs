using System;
using UnityEngine;

public class Room : MonoBehaviour {

	[SerializeField] private string id;
	[SerializeField] private Element obj;
	[SerializeField] private TapableObj tapable;

	private int currentLevel;

	public virtual void Init(RoomData roomData, Action onTap) {
		if (roomData.level != currentLevel) {
			Destroy(obj.gameObject);
			obj = Instantiate(Resources.Load<Element>($"Company/{id}/${id}"), transform);
			currentLevel = roomData.level;
		}
		tapable.SetOnTap(onTap);
	}
}

public class RoomData {
	public int level;
}
