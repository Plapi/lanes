using System.Collections.Generic;
using UnityEngine;

public class PathController : MonoBehaviour {

	[SerializeField] private UserCar userCar;
	
	private readonly List<Segment> segments = new();
	private readonly List<AICar> aiCars = new();

	private AICar[] aiCarPrefabs;
	
	private readonly LaneType[] segmentData = {
		LaneType.SideWalkLaneLeft,
		LaneType.RoadLaneSingleLeft,
		LaneType.RoadLaneMiddle,
		LaneType.RoadLaneEdgeLeft,
		LaneType.RoadLaneEdgeRight,
		LaneType.RoadLaneMiddle,
		LaneType.RoadLaneSingleRight,
		LaneType.SideWalkLaneRight
	};
	private const int segmentLength = 500;
	private int currentLength;

	private void Awake() {
		CreateNewSegment();
		const int roadLaneIndex = 4;
		if (segments[^1].TryGetLane(roadLaneIndex, out RoadLane lane)) {
			userCar.SetRoadLane(lane, roadLaneIndex);
		}
		userCar.Init(() => {
			CreateNewSegment();
			if (segments[^1].TryGetLane(userCar.RoadLaneIndex, out RoadLane roadLane)) {
				userCar.SetRoadLane(roadLane, userCar.RoadLaneIndex);
			}
		});
		InitAICars();
	}

	private void InitAICars() {
		aiCarPrefabs = Resources.LoadAll<AICar>("AICars");
		for (int i = 0; i < aiCarPrefabs.Length; i++) {
			aiCarPrefabs[i].Id = $"AICar{i}";
			ObjectPoolManager.CreatePool(aiCarPrefabs[i], 1);
		}
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
			TrySwitchLane(-1);
		} else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
			TrySwitchLane(1);
		}
		bool accelerate = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
		bool brake = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
		userCar.UpdateCar(accelerate, brake);

		if (Input.GetKeyDown(KeyCode.C)) {
			AICar carPrefab = aiCarPrefabs[Random.Range(0, aiCarPrefabs.Length)];
			AICar aiCar = ObjectPoolManager.Get(carPrefab, transform);
			aiCar.name = carPrefab.name;
			
			List<RoadLane> currentRoadLanes = GetCurrentRoadLanes();
			int randomRoadLaneIndex = Random.Range(0, currentRoadLanes.Count);
			
			aiCar.transform.localPosition = new Vector3(
				currentRoadLanes[randomRoadLaneIndex].transform.localPosition.x + Settings.Instance.laneSize / 2f,
				carPrefab.transform.localPosition.y, userCar.transform.localPosition.z + 100f);
			aiCar.SetRoadLane(currentRoadLanes[randomRoadLaneIndex], randomRoadLaneIndex);
			aiCar.Init(() => {
				if (segments[^1].TryGetLane(userCar.RoadLaneIndex, out RoadLane roadLane)) {
					userCar.SetRoadLane(roadLane, userCar.RoadLaneIndex);
				}
			});
			aiCar.gameObject.SetActive(true);
			
			aiCars.Add(aiCar);
		}

		if (Input.GetKeyDown(KeyCode.X)) {
			for (int i = 0; i < aiCars.Count; i++) { 
				ObjectPoolManager.Release(aiCars[i]); 
				aiCars.RemoveAt(i); 
				i--;
			}
		}

		for (int i = 0; i < aiCars.Count; i++) {
			if (aiCars[i].RoadLane == null) {
				ObjectPoolManager.Release(aiCars[i]);
				aiCars.RemoveAt(i);
				i--;
				continue;
			}
			aiCars[i].UpdateCar(false, false);
		}
	}

	private List<RoadLane> GetCurrentRoadLanes() {
		List<RoadLane> roadLanes = new();
		for (int i = 0; i < segmentData.Length; i++) {
			if (segments[^1].TryGetLane(i, out RoadLane roadLane)) {
				roadLanes.Add(roadLane);
			}
		}
		return roadLanes;
	}
	
	private void TrySwitchLane(int add) {
		int newLaneIndex = userCar.RoadLaneIndex + add;
		if (segments[^1].TryGetLane(newLaneIndex, out RoadLane roadLane)) {
			userCar.SetRoadLane(roadLane, newLaneIndex);
		}
	}

	private void LateUpdate() {
		if (segments.Count > 1 && userCar.GetCarReleaseSegmentPos().z > segments[0].transform.localPosition.z + segmentLength) {
			segments[0].ClearLanes();
			Destroy(segments[0].gameObject);
			segments.RemoveAt(0);
		}
	}

	private void CreateNewSegment() {
		segments.Add(GetSegment());
		currentLength += segmentLength;
	}

	private Segment GetSegment() {
		Segment segment = new GameObject("Segment").AddComponent<Segment>();
		segment.transform.parent = transform;
		segment.transform.SetLocalZ(currentLength);
		segment.SetLanes(segmentData, segmentLength);
		return segment;
	}
}
