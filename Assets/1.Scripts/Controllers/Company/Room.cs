using System;
using UnityEngine;

public class Room : MonoBehaviour {

	[SerializeField] private string id;
	[SerializeField] private Element roomElement;
	[SerializeField] private TapableObj tapable;
	[SerializeField] private Vector3 cameraZoom;
	[SerializeField] private GameObject upgradeParticles;

	private int currentLevel = -1;
	
	public RoomData RoomData { get; private set; }

	public void Init(RoomData roomData, Action onTap) {
		RoomData = roomData;
		tapable.SetOnTap(onTap);
	}

	public void SetRoomGraphic() {
		if (RoomData.level == currentLevel) {
			tapable.gameObject.SetActive(true);
			return;
		}
		
		ClearRoomGraphic();
		tapable.gameObject.SetActive(true);
		
		string objName = $"{id}{RoomData.level - 1}";
		Element elementPrefab = Resources.Load<Element>($"Company/{id}/{objName}/{objName}");
		if (elementPrefab == null) {
			return;
		}
		roomElement = ObjectPoolManager.Get(elementPrefab, transform);
		roomElement.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		roomElement.gameObject.SetActive(true);
		
		currentLevel = RoomData.level;
	}
	
	public void ClearRoomGraphic() {
		if (roomElement != null) {
			ObjectPoolManager.Release(roomElement);
			roomElement = null;
		}
		tapable.gameObject.SetActive(false);
		currentLevel = -1;
	}

	public void PlayParticles() {
		GameObject particles = Instantiate(upgradeParticles, upgradeParticles.transform.parent);
		particles.SetActive(true);
		Destroy(particles.gameObject, 1.7f);
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
	public new ParkingRoomDesignData design {
		get => (ParkingRoomDesignData)base.design;
		set => base.design = value;
	}
	public ParkingSlotData[] parkingSlots;

	public void UnlockNewSlot() {
		for (int i = 0; i < parkingSlots.Length; i++) {
			if (!parkingSlots[i].slotUnlocked) {
				parkingSlots[i].slotUnlocked = true;
				return;
			}
		}
	}

	public int GetTotalTaxiCount() {
		int count = 0;
		for (int i = 0; i < parkingSlots.Length; i++) {
			if (parkingSlots[i].taxiPurchased) {
				count++;
			}
		}
		return count;
	}
}