using System;
using UnityEngine;

public class VaultRoom : Room {
    
	[SerializeField] private VaultTable[] tables;

	public override void UpdateRoomGraphic(bool playParticles = true) {
		base.UpdateRoomGraphic(playParticles);
		UpdateTables(PlayerPrefsManager.UserData.coins);
	}

	public void UpdateTables(int coins) {
		for (int i = 0; i < RoomData.level; i++) {
			tables[i].Init(coins);
			coins = Mathf.Max(coins - ((VaultRoomData)RoomData).design.maxTableMoney, 0);
		}
	}
}
