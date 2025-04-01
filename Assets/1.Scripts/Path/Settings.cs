using UnityEngine;

public class Settings : ScriptableObjectSingleton<Settings> {
	
	public int laneSize;
	public int spawnAICarDistanceMin;
	public int spawnAICarDistanceMax;
	public AICar[] aiCarPrefabs;

	[Space]
	public bool testMode;
	public bool enableAdds;
	public bool enableAnalytics;
}
