using System;
using System.Collections.Generic;
using UnityEngine;

public class CompanyController : MonoBehaviour {
	
	[SerializeField] private StartSegment startSegment;
	[SerializeField] private GameObject roof;
	[SerializeField] private Transform building;
	[SerializeField] private ParkingRoom parkingRoom;
	[SerializeField] private List<Floor> floors;
	[SerializeField] private AudioClip showRoofAudioClip;
	[SerializeField] private AudioClip hideRoofAudioClip;
	
	public ParkingRoom ParkingRoom => parkingRoom;

	public void Init(Action<Room> onRoomTap) {
		PlayerPrefsManager.UserData.parkingRoom.design = Settings.Instance.company.parkingRoom;
		for (int i = 0; i < PlayerPrefsManager.UserData.drivers.Length; i++) {
			PlayerPrefsManager.UserData.drivers[i].design = Settings.Instance.company.drivers[i];
		}

		for (int i = 0; i < PlayerPrefsManager.UserData.floors.Length; i++) {
			if (i == floors.Count) {
				floors.Add(Instantiate(floors[0], floors[0].transform.parent));
			}
		}
		for (int i = 0; i < floors.Count; i++) {
			floors[i].Init(i, onRoomTap);
		}
		SetRoofHeight();
		roof.SetActive(true);
		
		parkingRoom.Init(PlayerPrefsManager.UserData.parkingRoom, () => {
			onRoomTap?.Invoke(parkingRoom);
		});
		parkingRoom.SetRoomGraphic();
		
		Activate();
	}

	public void UpgradeFloor(Action<Room> onRoomTap) {
		floors.Add(Instantiate(floors[0], floors[0].transform.parent));
		floors[^1].ClearRoomsGraphic();
		floors[^1].Init(floors.Count - 1, onRoomTap);
		floors[^1].PlayParticles();
		UpdateFloorGraphic(floors.Count - 1);
		SetRoofHeight();
	}

	public void UpdateFloorLevel(int floorLevel) {
		for (int i = 0; i < floors.Count; i++) {
			floors[i].gameObject.SetActive(i <= floorLevel);
		}
	}

	public void UpdateFloorGraphic(int floorLevel) {
		if (floorLevel >= floors.Count) {
			return;
		}
		for (int i = 0; i < floors.Count; i++) {
			if (i >= floorLevel) {
				floors[i].SetRoomsGraphic();
			} else {
				floors[i].ClearRoomsGraphic();
			}
		}
	}

	public void UpdateVaultTables(int coins) {
		for (int i = 0; i < floors.Count; i++) {
			coins = floors[i].VaultRoom.UpdateTables(coins);
		}
	}

	public void Activate() {
		startSegment.Init(Segment.GetSegmentData(new SegmentInputData { length = 200 }));
		startSegment.SetStartAndEndPosForRoadLanes();
		startSegment.SpawnAICars();
		ParkingRoom.Activate(startSegment);
	}

	public void Deactivate() {
		startSegment.ClearAICars();
		ParkingRoom.Deactivate();
	}
	
	public void UpdateVisibility(Transform camera, Action<bool> onChangeVisibility) {
		Vector3 pos = new Vector3(building.position.x, building.position.y, camera.position.z);
		Utils.GetIntersection(camera.position, camera.position + Vector3.right * 100f, pos, pos + Vector3.up * 100f, out Vector3 intersection);
		float dist = Vector3.Distance(intersection, camera.position);
		bool roofActive = dist > 80f;
		if (roof.activeSelf != roofActive) {
			roof.SetActive(roofActive);
			onChangeVisibility(roofActive);
			if (roofActive) {
				UpdateFloorLevel(floors.Count);
				for (int i = 0; i < floors.Count; i++) {
					floors[i].ClearRoomsGraphic();
				}
			} else {
				floors[^1].SetRoomsGraphic();
			}
			AudioSystem.Play(roofActive ? showRoofAudioClip : hideRoofAudioClip);
			HapticFeedback.VibrateHaptic(HapticFeedback.Type.Light);
		}
	}

	private void SetRoofHeight() {
		roof.transform.SetY((PlayerPrefsManager.UserData.floors.Length - 1) * Settings.Instance.company.floorHeight);
	}
}

[Serializable]
public class DriverData {
	[NonSerialized] public DriverDesignData design;
	public bool hired;
}
