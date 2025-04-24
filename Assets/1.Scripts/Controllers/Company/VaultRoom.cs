using System;
using UnityEngine;

public class VaultRoom : Room {
    
	[SerializeField] private VaultTable[] tables;

	public override void Init(RoomData roomData, Action onTap) {
		base.Init(roomData, onTap);
		int money = PlayerPrefsManager.UserData.coins;
		for (int i = 0; i < roomData.level; i++) {
			tables[i].Init(money);
			money = Mathf.Max(money - Settings.Instance.company.vaultRoom.maxTableMoney, 0);
		}
	}
}
