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
	public VaultRoomDesignData vaultRoom;
}

[Serializable]
public class VaultRoomDesignData {
	public int[] tableMoneyLevels;
	public int maxTableMoney;
}
