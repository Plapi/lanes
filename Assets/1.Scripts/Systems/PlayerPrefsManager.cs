using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public static class PlayerPrefsManager {

	private const string userDataKey = "GameUserData";
	private static UserData userData;

	public static UserData UserData {
		get {
			if (userData == null) {
				if (PlayerPrefs.HasKey(userDataKey)) {
					string json = PlayerPrefs.GetString(userDataKey);
					userData = JsonUtility.FromJson<UserData>(json);
				} else {
					SetNoBackup();
					userData = UserData.GetDefault();
				}
			}
			return userData;
		}
	}

	public static void SaveUserData() {
		string json = JsonUtility.ToJson(UserData);
		PlayerPrefs.SetString(userDataKey, json);
		PlayerPrefs.Save();
	}

	private static void SetNoBackup() {
#if UNITY_IOS
		string path = System.IO.Path.Combine(Application.persistentDataPath, $"../Library/Preferences/{Application.identifier}.plist");
		if (!string.IsNullOrEmpty(path)) {
			UnityEngine.iOS.Device.SetNoBackupFlag(path);
		}
#endif
	}
}

[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
[Serializable]
public class UserData {
	
	public bool isTutorialDone;
	public int coins = 500;
	public List<int> unlockedCars = new() { 0 };
	public List<int> carColors = new() { 7, 4, 0, 0, 9, 1 };
	public int carSelection;
	public int distanceBest = -1;
	public int personsBest = -1;
	public float[] volumes = { 1f, 0.3f, 0.4f };
	public bool hapticFeedback = true;
	
	public WatchAdBoostIncome watchAdBoostIncome;
	public SerializedDateTime lastCollectTime;
	
	// company
	public FloorData[] floors;
	public SerializedDateTime floorUpgradeTime;
	public bool inFloorUpgradeTime;
	public ParkingRoomData parkingRoom = new();
	public DriverData[] drivers;

	public int GetFlorReached() {
		return floors.Length - 1;
	}

	public void StartUpgradeFloor() {
		coins -= GetCurrentFloorUpgradeCost();
		floorUpgradeTime = new SerializedDateTime(DateTime.Now);
		inFloorUpgradeTime = true;
		PlayerPrefsManager.SaveUserData();
	}

	public int GetCurrentFloorUpgradeCost() {
		return Settings.Instance.company.floorUpgradeCost * (floors.Length + 1);
	}

	public void UpgradeFloor() {
		inFloorUpgradeTime = false;
		floors = new List<FloorData>(floors) { new() }.ToArray();
		PlayerPrefsManager.SaveUserData();
	}

	public int CalculateCapacity() {
		int capacity = 0;
		for (int i = 0; i < floors.Length; i++) {
			capacity += floors[i].vaultRoom.Capacity;
		}
		return capacity;
	}
	
	public void IncreaseCoins(int amount, bool useVaultCapacity = true) {
		if (useVaultCapacity) {
			amount = Mathf.Min(amount, CalculateCapacity() - coins);
		}
		if (amount > 0) {
			coins += amount;	
		}
		PlayerPrefsManager.SaveUserData();
	}

	public bool VaultIsFull() {
		return coins >= CalculateCapacity();
	}

	public static UserData GetDefault() {
		UserData userData = new() {
			floors = new[] { new FloorData() },
			parkingRoom = {
				parkingSlots = new ParkingSlotData[10]
			}
		};
		for (int i = 0; i < userData.parkingRoom.parkingSlots.Length; i++) {
			userData.parkingRoom.parkingSlots[i] = new ParkingSlotData();
		}
		userData.parkingRoom.UnlockNewSlot();
		userData.drivers = new DriverData[Settings.Instance.company.drivers.Length];
		for (int i = 0; i < userData.drivers.Length; i++) {
			userData.drivers[i] = new DriverData();
		}
		return userData;
	}
	
	public bool TryGetTotalIncomeFromLastCollect(out int income, out int seconds) {
		if (lastCollectTime == null || !TryGetCoinsIncome(out income)) {
			income = 0;
			seconds = 0;
			return false;
		}
		seconds = Mathf.RoundToInt((float)(DateTime.Now - lastCollectTime.Date).TotalSeconds);
		int totalTurns = seconds / Settings.Instance.company.incomeTurnDuration;
		income = Mathf.Min(income * totalTurns, CalculateCapacity() - coins);
		return true;
	}

	public bool TryGetCoinsIncome(out int income) {
		List<RoomData> rooms = new();
		for (int i = 0; i < floors.Length; i++) {
			rooms.Add(floors[i].waitingRoom);
			rooms.Add(floors[i].callCenterRoom);
			rooms.Add(floors[i].breakRoom);
		}
		income = 0;
		for (int i = 0; i < rooms.Count; i++) {
			income += rooms[i].CoinsIncome;
		}
		for (int i = 0; i < parkingRoom.parkingSlots.Length; i++) {
			if (parkingRoom.parkingSlots[i].HasDriver) {
				income += GetDriver(parkingRoom.parkingSlots[i].driverId).design.income;
			}
		}
		if (watchAdBoostIncome != null && watchAdBoostIncome.endTime.Date >= DateTime.Now) {
			income *= 2;
		}
		return income > 0;
	}
	
	public DriverData GetDriver(string id) {
		for (int i = 0; i < drivers.Length; i++) {
			if (drivers[i].design.id == id) {
				return drivers[i];
			}
		}
		return null;
	}

	public bool TryGetParkingSlotIndex(DriverData driver, out int index) {
		index = -1;
		for (int i = 0; i < parkingRoom.parkingSlots.Length; i++) {
			if (parkingRoom.parkingSlots[i].driverId == driver.design.id) {
				index = i;
				return true;
			}
		}
		return false;
	}

	public void ReachToMax() {
		for (int i = 0; i < floors.Length; i++) {
			floors[i].waitingRoom.level = 10;
			floors[i].vaultRoom.level = 10;
			floors[i].callCenterRoom.level = 10;
			floors[i].breakRoom.level = 10;
		}
		parkingRoom.level = 10;
		for (int i = 0; i < parkingRoom.parkingSlots.Length; i++) {
			parkingRoom.parkingSlots[i].slotUnlocked = true;
			parkingRoom.parkingSlots[i].taxiPurchased = true;
		}
		PlayerPrefsManager.UserData.coins = 0;
		IncreaseCoins(CalculateCapacity());
		PlayerPrefsManager.SaveUserData();
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
	}
}