using System;
using UnityEngine;
using UnityEngine.Purchasing;

public class Settings : ScriptableObjectSingleton<Settings> {
	
	public int laneSize;
	
	[Space]
	public AICar[] aiCarPrefabs;
	
	[Space]
	public Sprite[] personSprites;

	[Space]
	public CompanyData company;
	
	[Space]
	public InAppPurchaseProduct[] inAppPurchaseProducts;
	
	[Space]
	public bool testMode;
	public bool enableAdds;
	public bool enableAnalytics;
}

[Serializable]
public class CompanyData {
	public int incomeTurnDuration;
	public int maxFloors;
	public int floorUpgradeCost;
	public int floorUpgradeDurationMinutes;
	public float floorHeight;
	public RoomDesignData waitingRoom;
	public RoomDesignData breakRoom;
	public RoomDesignData callCenterRoom;
	public VaultRoomDesignData vaultRoom;
	public ParkingRoomDesignData parkingRoom;
	public DriverDesignData[] drivers;
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

[Serializable]
public class DriverDesignData {
	public string id;
	public string name;
	public int hireCost;
	public int fireCost => hireCost / 2;
	public int income;
	public int stars;
	public string spritePath => $"Company/Drivers/Driver{id}";
}

[Serializable]
public class InAppPurchaseProduct {
	public string id;
	public int value;
	public ProductType productType;
}