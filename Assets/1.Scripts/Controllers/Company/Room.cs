using System;
using UnityEngine;

public class Room : MonoBehaviour {

	[SerializeField] private string id;
	[SerializeField] private Element obj;
	[SerializeField] private TapableObj tapable;
	[SerializeField] private Vector3 cameraZoom;
	[SerializeField] private GameObject upgradeParticles;

	private int currentLevel = 1;
	
	public RoomData RoomData { get; private set; }

	public void Init(RoomData roomData, Action onTap) {
		RoomData = roomData;
		UpdateRoomGraphic(false);
		tapable.SetOnTap(onTap);
	}

	public virtual void UpdateRoomGraphic(bool playParticles = true) {
		if (RoomData.level != currentLevel) {
			Destroy(obj.gameObject);
			string objName = $"{id}{RoomData.level - 1}";
			obj = Instantiate(Resources.Load<Element>($"Company/{id}/{objName}/{objName}"), transform);
			currentLevel = RoomData.level;
			if (playParticles) {
				upgradeParticles.SetActive(true);
				this.Wait(1.7f, () => upgradeParticles.SetActive(false));
			}
		}
	}

	public Vector3 GetCameraZoom() {
		return cameraZoom;
	}
}

[Serializable]
public class RoomData {
	[NonSerialized] public RoomDesignData design;
	public int level = 1;
	public int MaxLevel => design.upgradeCosts.Length;
	public bool MaxLevelReached => level - 1 >= MaxLevel;
	public int CoinsIncome => design.cashIncomes[level - 1];
	public int UpgradeCost => design.upgradeCosts[level - 1];
}

[Serializable]
public class VaultRoomData : RoomData {
	public new VaultRoomDesignData design => (VaultRoomDesignData)base.design;
	public int Capacity => level * design.maxTableMoney;
}

[Serializable]
public class ParkingRoomData : RoomData {
	public new ParkingRoomDesignData design => (ParkingRoomDesignData)base.design;
	public ParkingSlotData[] parkingSlots;

	public void UnlockNewSlot() {
		for (int i = 0; i < parkingSlots.Length; i++) {
			if (!parkingSlots[i].slotUnlocked) {
				parkingSlots[i].slotUnlocked = true;
				return;
			}
		}
	}
}

[Serializable]
public class ParkingSlotData {
	public bool slotUnlocked;
	public bool taxiPurchased;
	public int taxiId;
}