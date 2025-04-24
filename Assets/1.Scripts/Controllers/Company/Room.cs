using System;
using JsonFx.Json;
using UnityEngine;

public class Room : MonoBehaviour {

	[SerializeField] private string id;
	[SerializeField] private Element obj;
	[SerializeField] private TapableObj tapable;
	[SerializeField] private Vector3 cameraZoom;

	private int currentLevel = 1;
	
	public RoomData RoomData { get; private set; }

	public virtual void Init(RoomData roomData, Action onTap) {
		RoomData = roomData;
		if (roomData.level != currentLevel) {
			Destroy(obj.gameObject);
			obj = Instantiate(Resources.Load<Element>($"Company/{id}/{id}/${id}{roomData.level - 1}"), transform);
			currentLevel = roomData.level;
		}
		tapable.SetOnTap(onTap);
	}

	public Vector3 GetCameraZoom() {
		return cameraZoom;
	}
}

public class RoomData {
	[JsonIgnore] public RoomDesignData design;
	public int level = 1;
	public int MaxLevel => design.upgradeCosts.Length;
	public bool MaxLevelReached => level >= MaxLevel;
}

public class VaultRoomData : RoomData {
	[JsonIgnore] public new VaultRoomDesignData design => (VaultRoomDesignData)base.design;
	public int Capacity => level * design.maxTableMoney;
}
