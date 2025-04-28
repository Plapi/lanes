using System;
using UnityEngine;

public class Settings : ScriptableObjectSingleton<Settings> {
	
	public int laneSize;
	
	[Space]
	public AICar[] aiCarPrefabs;
	
	[Space]
	public Sprite[] personSprites;

	[Space]
	public CompanyData company;
	
	[Space]
	public bool testMode;
	public bool enableAdds;
	public bool enableAnalytics;
}

[Serializable]
public class CompanyData {
	public RoomDesignData waitingRoom;
	public RoomDesignData breakRoom;
	public RoomDesignData callCenterRoom;
	public VaultRoomDesignData vaultRoom;
	public ParkingRoomDesignData parkingRoom;
}

[Serializable]
public class RoomDesignData {
	public string name;
	public int[] upgradeCosts;
	public int[] cashIncomes;
}

[Serializable]
public class VaultRoomDesignData : RoomDesignData {
	public int[] tableMoneyLevels;
	public int maxTableMoney;
}

[Serializable]
public class ParkingRoomDesignData : RoomDesignData {
	public int taxiCost;
	public int taxiIncome;
}
