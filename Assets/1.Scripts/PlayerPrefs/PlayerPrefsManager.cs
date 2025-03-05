using UnityEngine;

public static class PlayerPrefsManager {
	
	private const string userDataKey = "UserData";
	private static UserData userData;
	
	public static UserData UserData {
		get {
			if (userData == null) {
				if (PlayerPrefs.HasKey(userDataKey)) {
					string json = PlayerPrefs.GetString(userDataKey);
					userData = JsonUtility.FromJson<UserData>(json);
				} else {
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
}

public class UserData {
	public int coins;
	public int carSelection;
	public int distanceBest = -1;
	public int personsBest = -1;
}