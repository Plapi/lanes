using UnityEngine;

public class Settings : ScriptableObjectSingleton<Settings> {
	
	public int laneSize;
	
	[Space]
	public AICar[] aiCarPrefabs;
	
	[Space]
	public Sprite[] personSprites;
	
	[Space]
	public bool testMode;
	public bool enableAdds;
	public bool enableAnalytics;
}
