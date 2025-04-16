using UnityEngine;

public class VaultRoom : Room {
    
	[SerializeField] private VaultTable[] tables;

	public override void Init(RoomData roomData) {
		base.Init(roomData);
		VaultRoomData vaultRoomData = (VaultRoomData)roomData;
		int money = PlayerPrefsManager.UserData.coins;
		for (int i = 0; i <= vaultRoomData.tableMoneyLevel; i++) {
			tables[i].Init(money);
			money = Mathf.Max(money - Settings.Instance.company.vaultRoom.maxTableMoney, 0);
		}
	}
}

public class VaultRoomData : RoomData {
	public int tableMoneyLevel;
}
