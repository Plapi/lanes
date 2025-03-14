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
					userData = new UserData();
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
public class UserData {
	public bool isTutorialDone;
	public int coins;
	public List<int> unlockedCars = new() { 0 };
	public List<int> carColors = new() { 7, 4, 0, 0, 9, 1 };
	public int carSelection;
	public int distanceBest = -1;
	public int personsBest = -1;
	public float[] volumes = { 1f, 0.3f, 0.4f };
	public bool hapticFeedback = true;
}