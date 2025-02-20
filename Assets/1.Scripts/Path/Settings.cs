using System.Collections.Generic;
using UnityEngine;

public class Settings : ScriptableObjectSingleton<Settings> {
	
	public int laneSize;
	public int spawnAICarDistanceMin;
	public int spawnAICarDistanceMax;
	public AICar[] aiCarPrefabs;
	public Environment environment;
	
	[System.Serializable]
	public class Environment {
		public Element sideWalk;
		public Building[] buildings;
		[Range(0f, 1f)] public float[] buildingRandomProbabilities;

		public bool TryGetRandomBuilding(int maxLength, out Building building) {
			// building = buildings[4];
			// return building.Length <= maxLength;
			List<Building> list = new();
			List<float> probabilities = new();
			for (int i = 0; i < buildings.Length; i++) {
				if (buildings[i].Length <= maxLength) {
					list.Add(buildings[i]);
					probabilities.Add(buildingRandomProbabilities[i]);
				}
			}
			building = list.Count > 0 ? Utils.SelectRandomItem(list.ToArray(), probabilities.ToArray()): null;
			return building != null;
		}
	}
}
