using System;
using UnityEngine;

public class CompanyController : MonoBehaviour {
	
	[SerializeField] private StartSegment startSegment;
	[SerializeField] private ParkingController parkingController;

	[Space]
	[SerializeField] private Room[] rooms;

	public void Init(Action<Room> onRoomTap) {
		Activate();
		RoomData[] roomData = {
			PlayerPrefsManager.UserData.waitingRoom,
			PlayerPrefsManager.UserData.vaultRoom
		};
		roomData[0].design = Settings.Instance.company.waitingRoom;
		roomData[1].design = Settings.Instance.company.vaultRoom;
		for (int i = 0; i < rooms.Length; i++) {
			int ii = i;
			rooms[i].Init(roomData[i], () => {
				onRoomTap(rooms[ii]);
			});
		}
	}

	public void Activate() {
		startSegment.Init(Segment.GetSegmentData(new SegmentInputData { length = 200 }));
		startSegment.SetStartAndEndPosForRoadLanes();
		startSegment.SpawnAICars();
		parkingController.Activate(startSegment);
	}

	public void Deactivate() {
		startSegment.ClearAICars();
		parkingController.Deactivate();
	}
}
