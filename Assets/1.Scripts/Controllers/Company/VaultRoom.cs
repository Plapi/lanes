using UnityEngine;

public class VaultRoom : Room {
    
	[SerializeField] private VaultTable[] tables;
	[SerializeField] private int depositedCoins;
	
	public int UpdateTables(int coins) {
		depositedCoins = 0;
		VaultRoomData roomData = (VaultRoomData)RoomData;
		int coinsPerTable = roomData.Design.maxTableMoney;
		for (int i = 0; i < RoomData.level; i++) {
			tables[i].Init(coins);
			if (coins >= coinsPerTable) {
				depositedCoins += coinsPerTable;
			} else {
				depositedCoins += coins;
			}
			coins = Mathf.Max(coins - coinsPerTable, 0);
		}
		roomData.depositedCoins = depositedCoins;
		return coins;
	}
	
#if UNITY_EDITOR
	[ContextMenu("Set Tables")]
	private void SetTables() {
		for (int i = 0; i < tables.Length; i++) {
			tables[i].Init(int.MaxValue);
		}
	}
	[ContextMenu("Remove Tables")]
	private void RemoveTables() {
		for (int i = 0; i < tables.Length; i++) {
			DestroyImmediate(tables[i].transform.GetChild(0).gameObject);
		}
	}
#endif
}
