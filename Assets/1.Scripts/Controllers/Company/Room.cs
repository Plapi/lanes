using System;
using UnityEngine;
using TMPro;

public class Room : MonoBehaviour {

	[SerializeField] private string id;
	[SerializeField] private Element roomElement;
	[SerializeField] private TapableObj tapable;
	[SerializeField] private Vector3 cameraZoom;
	[SerializeField] private GameObject upgradeParticles;
	
	[Space]
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private GameObject upgradeObject;
	[SerializeField] private TextMeshProUGUI text;

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

	public void UpdateUpgradeObject() {
		int coins = PlayerPrefsManager.UserData.coins;
		bool canUpgrade = !RoomData.MaxLevelReached && coins >= RoomData.UpgradeCost;
		upgradeObject.SetActive(canUpgrade);
	}

	public void UpdateUITextScale(Transform cameraTransform) {
		float y = cameraTransform.position.y;
		bool active = y <= 30f;
		if (text.transform.parent.gameObject.activeSelf != active) {
			text.transform.parent.gameObject.SetActive(active);
		}
		if (active) {
			text.fontSize = Mathf.Lerp(60f, 100f, Mathf.InverseLerp(10f, 30f, y));
			
			Utils.GetIntersection(cameraTransform.position, cameraTransform.position + Vector3.right * 100f,
				text.transform.position, text.transform.position + Vector3.up * 100f, out Vector3 intersection);
			float dist = Vector3.Distance(intersection, cameraTransform.position);
			canvasGroup.alpha = Mathf.Lerp(0.5f, 1f, Mathf.InverseLerp(10f, 30f, dist));
		}
	}
}

[Serializable]
public class RoomData {
	
	public int level = 1;
	public int MaxLevel => Design.upgradeCosts.Length;
	public bool MaxLevelReached => level - 1 >= MaxLevel;
	public int CoinsIncome => Design.cashIncomes[level - 1];
	
	private int floorLevel;
	
	public RoomDesignData Design { get; private set; }
	public int UpgradeCost => Design.upgradeCosts[level - 1] * (floorLevel + 1);
	
	public void Init(RoomDesignData design, int floorLevel = 0) {
		Design = design;
		this.floorLevel = floorLevel;
	}
}

[Serializable]
public class VaultRoomData : RoomData {
	public new VaultRoomDesignData Design => (VaultRoomDesignData)base.Design;
	public int Capacity => level * Design.maxTableMoney;
	
	[NonSerialized] public int depositedCoins;
}

[Serializable]
public class ParkingRoomData : RoomData {
	
	public new ParkingRoomDesignData Design => (ParkingRoomDesignData)base.Design;
	
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

	public int GetParkingSlotIndex(ParkingSlotData parkingSlot) {
		for (int i = 0; i < parkingSlots.Length; i++) {
			if (parkingSlots[i] == parkingSlot) {
				return i;
			}
		}
		return -1;
	}
}