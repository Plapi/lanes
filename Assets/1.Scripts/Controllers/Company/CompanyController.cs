using System;
using UnityEngine;

public class CompanyController : MonoBehaviour {
	
	[SerializeField] private StartSegment startSegment;
	[SerializeField] private GameObject roof;
	[SerializeField] private Transform building;

	[Space]
	[SerializeField] private Room[] rooms;

	public VaultRoom VaultRoom => (VaultRoom)rooms[1];
	public ParkingRoom ParkingRoom => (ParkingRoom)rooms[^1];

	public void Init(Action<Room> onRoomTap) {
		RoomData[] roomData = {
			PlayerPrefsManager.UserData.waitingRoom,
			PlayerPrefsManager.UserData.vaultRoom,
			PlayerPrefsManager.UserData.callCenterRoom,
			PlayerPrefsManager.UserData.breakRoom,
			PlayerPrefsManager.UserData.parkingRoom
		};
		roomData[0].design = Settings.Instance.company.waitingRoom;
		roomData[1].design = Settings.Instance.company.vaultRoom;
		roomData[2].design = Settings.Instance.company.callCenterRoom;
		roomData[3].design = Settings.Instance.company.breakRoom;
		roomData[4].design = Settings.Instance.company.parkingRoom;
		for (int i = 0; i < PlayerPrefsManager.UserData.drivers.Length; i++) {
			PlayerPrefsManager.UserData.drivers[i].design = Settings.Instance.company.drivers[i];
		}
		for (int i = 0; i < rooms.Length; i++) {
			int ii = i;
			rooms[i].Init(roomData[i], () => {
				onRoomTap(rooms[ii]);
			});
		}
		Activate();
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

	public void UpdateRoof(Transform camera) {
		Vector3 pos = new Vector3(building.position.x, building.position.y, camera.position.z);
		// GizmosController.Instance.DrawLine("20", camera.position, camera.position + Vector3.right * 100f, Color.red);
		// GizmosController.Instance.DrawLine("21", pos, pos + Vector3.up * 100f, Color.green);
		Utils.GetIntersection(camera.position, camera.position + Vector3.right * 100f, pos, pos + Vector3.up * 100f, out Vector3 intersection);
		float dist = Vector3.Distance(intersection, camera.position);
		bool roofActive = dist > 80f;
		if (roof.activeSelf != roofActive) {
			roof.SetActive(roofActive);
		}
	}
}

[Serializable]
public class DriverData {
	[NonSerialized] public DriverDesignData design;
	public bool hired;
}
