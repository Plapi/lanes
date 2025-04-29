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
	public RoomData waitingRoom = new();
	public VaultRoomData vaultRoom = new();
	public RoomData callCenterRoom = new();
	public RoomData breakRoom = new();
	public ParkingRoomData parkingRoom = new();
	public DriverData[] drivers;

	public void IncreaseCoins(int amount) {
		coins += amount;
		coins = Mathf.Min(coins, vaultRoom.Capacity);
		PlayerPrefsManager.SaveUserData();
	}

	public bool VaultIsFull() {
		return coins >= vaultRoom.Capacity;
	}

	public static UserData GetDefault() {
		UserData userData = new() {
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

	public bool TryGetCoinsIncome(out int income) {
		RoomData[] roomData = {
			PlayerPrefsManager.UserData.waitingRoom,
			PlayerPrefsManager.UserData.callCenterRoom,
			PlayerPrefsManager.UserData.breakRoom
		};
		income = 0;
		for (int i = 0; i < roomData.Length; i++) {
			income += roomData[i].CoinsIncome;
		}
		for (int i = 0; i < parkingRoom.parkingSlots.Length; i++) {
			if (parkingRoom.parkingSlots[i].HasDriver) {
				income += GetDriver(parkingRoom.parkingSlots[i].driverId).design.income;
			}
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
		waitingRoom.level = 10;
		vaultRoom.level = 10;
		callCenterRoom.level = 10;
		breakRoom.level = 10;
		parkingRoom.level = 10;
		for (int i = 0; i < parkingRoom.parkingSlots.Length; i++) {
			parkingRoom.parkingSlots[i].slotUnlocked = true;
			parkingRoom.parkingSlots[i].taxiPurchased = true;
		}
		PlayerPrefsManager.UserData.coins = 0;
		IncreaseCoins(vaultRoom.Capacity);
		PlayerPrefsManager.SaveUserData();
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
	}
}