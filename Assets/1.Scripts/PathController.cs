using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PathController : MonoBehaviour {

	[SerializeField] private InputManager inputManager;
	[SerializeField] private UserCar userCar;
	[SerializeField] private Circuit circuit;
	
	private readonly List<Segment> segments = new();
	private readonly List<AICar> aiCars = new();

	private AICar[] aiCarPrefabs;
	
	private int currentLength;

	private void Awake() {
		CreateNewSegment();
		userCar.SetSegment(segments[^1], segments[^1].RoadLanes.Count - 1);
		userCar.gameObject.SetActive(true);
		InitAICars();

		inputManager.OnHorizontalInput = input => {
			userCar.TrySwitchLane((int)Mathf.Sign(input));
		};
		
		StartCoroutine(SpawnMechanic());
	}

	private void InitAICars() {
		aiCarPrefabs = Resources.LoadAll<AICar>("AICars");
		for (int i = 0; i < aiCarPrefabs.Length; i++) {
			aiCarPrefabs[i].Id = $"AICar{i}";
			ObjectPoolManager.CreatePool(aiCarPrefabs[i], 1);
		}
	}

	private void Update() {
		userCar.UpdateCar(inputManager.VerticalInput);
		for (int i = 0; i < aiCars.Count; i++) {
			if (aiCars[i].CurrentRoadLane == null) {
				ObjectPoolManager.Release(aiCars[i]);
				aiCars.RemoveAt(i);
				i--;
				continue;
			}
			aiCars[i].UpdateCar(0f);
		}
		
		if (Input.GetKeyDown(KeyCode.C)) {
			SpawnAICar();
		}
		if (Input.GetKeyDown(KeyCode.X)) {
			for (int i = 0; i < aiCars.Count; i++) { 
				ObjectPoolManager.Release(aiCars[i]); 
				aiCars.RemoveAt(i); 
				i--;
			}
		}
	}

	private IEnumerator SpawnMechanic() {
		while (true) {
			yield return new WaitForSeconds(Random.Range(1f, 2f));
			SpawnAICar();
		}
	}

	private void LateUpdate() {
		if (userCar.GetRequireNewSegmentPos().z > segments[^1].transform.position.z + segments[^1].SegmentData.length) {
			CreateNewSegment();
		}
		if (userCar.transform.position.z + 4f > userCar.CurrentRoadLane.transform.position.z + userCar.CurrentRoadLane.Length) {
			userCar.SetSegment(segments[^1], userCar.RoadLaneIndex);
		}
		if (segments.Count > 1 && userCar.GetCarReleaseSegmentPos().z > segments[0].transform.localPosition.z + segments[0].SegmentData.length) {
			segments[0].ClearLanes();
			Destroy(segments[0].gameObject);
			segments.RemoveAt(0);
		}

		for (int i = 0; i < aiCars.Count; i++) {
			
			if (!aiCars[i].CurrentRoadLane.Data.hasFrontDirection && userCar.transform.position.z - aiCars[i].transform.position.z > 10f) {
				ObjectPoolManager.Release(aiCars[i]);
				aiCars.RemoveAt(i);
				i--;
				continue;
			} 
			
			if (aiCars[i].CurrentRoadLane.Data.hasFrontDirection) {
				if (aiCars[i].transform.position.z + 4f > aiCars[i].CurrentRoadLane.transform.position.z + aiCars[i].CurrentRoadLane.Length) {
					int currentForwardStart = aiCars[i].CurrentSegment.BackRoadLanes.Count;
					int nextForwardStart = segments[^1].BackRoadLanes.Count;
					int nextLaneIndex = Mathf.Max(currentForwardStart, nextForwardStart);
					aiCars[i].SetSegment(segments[^1], nextLaneIndex);
				}
			} else {
				if (aiCars[i].transform.position.z - 4f < aiCars[i].CurrentRoadLane.transform.position.z) {
					int newLaneIndex = Mathf.Min(aiCars[i].RoadLaneIndex, segments[0].BackRoadLanes.Count - 1);
					aiCars[i].SetSegment(segments[0], newLaneIndex);
				}
			}
		}
	}

	private void SpawnAICar() {
		AICar carPrefab = aiCarPrefabs[Random.Range(0, aiCarPrefabs.Length)];
		AICar aiCar = ObjectPoolManager.Get(carPrefab, transform);
		aiCar.name = carPrefab.name;
		
		List<RoadLane> currentRoadLanes = segments[^1].RoadLanes;
		int randomRoadLaneIndex = Random.Range(0, currentRoadLanes.Count);
		//int randomRoadLaneIndex = segments[^1].BackRoadLanes.Count - 1;
		//int randomRoadLaneIndex = segments[^1].RoadLanes.Count - 1;
		//int randomRoadLaneIndex = segments[^1].BackRoadLanes.Count;
		RoadLane roadLane = currentRoadLanes[randomRoadLaneIndex];

		aiCar.transform.SetLocalZ(userCar.transform.localPosition.z + 100f);
		
		if (!roadLane.Data.hasFrontDirection) {
			aiCar.transform.SetAngleY(180f);
		}
		
		aiCar.SetSegment(segments[^1], randomRoadLaneIndex);
		aiCar.gameObject.SetActive(true);
		
		aiCars.Add(aiCar);
	}

	private void CreateNewSegment() {
		segments.Add(GetSegment());
		currentLength += segments[^1].SegmentData.length;
	}

	private Segment GetSegment() {
		Segment segment = new GameObject("Segment").AddComponent<Segment>();
		segment.transform.parent = transform;
		segment.transform.SetLocalZ(currentLength);
		segment.Init(GetNextSegmentData());
		return segment;
	}

	private SegmentData GetNextSegmentData() {
		List<LaneData> lanes = new() {
			new LaneData {
				type = LaneType.SideWalkLaneLeft
			}
		};

		SegmentInputData segmentInputData = circuit.GetNextSegment();
		int backLanes = segmentInputData.backLanes;
		int frontLanes = segmentInputData.frontLanes;
		
		lanes.Add(new RoadLaneData {
			type = LaneType.RoadLaneSingleLeft
		});
		if (backLanes > 1) {
			for (int i = 1; i < backLanes - 1; i++) {
				lanes.Add(new RoadLaneData {
					type = LaneType.RoadLaneMiddle
				});	
			}
			lanes.Add(new RoadLaneData {
				type = LaneType.RoadLaneEdgeLeft
			});
		}
		
		if (backLanes > 1) {
			lanes.Add(new RoadLaneData {
				type = LaneType.RoadLaneEdgeRight,
				hasFrontDirection = true,
			});
			frontLanes--;
		}
		if (frontLanes > 0) {
			for (int i = 1; i < frontLanes; i++) {
				lanes.Add(new RoadLaneData {
					type = LaneType.RoadLaneMiddle,
					hasFrontDirection = true
				});	
			}
			lanes.Add(new RoadLaneData {
				type = LaneType.RoadLaneSingleRight,
				hasFrontDirection = true,
			});
		}
		lanes.Add(new LaneData {
			type = LaneType.SideWalkLaneRight
		});
		
		return new SegmentData {
			lanes = lanes.ToArray(),
			length = 5 * Random.Range(20, 60)
		};
	}
	
	[Serializable]
	private class Circuit {
		public SegmentInputData[] segments;
		private int currentSegmentIndex;

		public SegmentInputData GetNextSegment() {
			if (currentSegmentIndex >= segments.Length) {
				currentSegmentIndex = 0;
			}
			return segments[currentSegmentIndex++];
		}
	}
	
	[Serializable]
	private class SegmentInputData {
		[Range(2, 4)] public int backLanes = 2;
		[Range(2, 4)] public int frontLanes = 2;
	}
}
