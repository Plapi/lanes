using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
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
		return userData;
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
		EditorApplication.isPlaying = false;
	}
}