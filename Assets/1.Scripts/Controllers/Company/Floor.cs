using System;
using UnityEngine;

public class Floor : MonoBehaviour {
	
	[SerializeField] private Room[] rooms;
	[SerializeField] private GameObject m_firstFloor;
	[SerializeField] private GameObject m_otherFloor;
	[SerializeField] private GameObject upgradeParticles;
	
	public VaultRoom VaultRoom => (VaultRoom)rooms[1];

	public void Init(int level, Action<Room> onRoomTap) {
		name = $"Floor{level}";
		
		m_firstFloor.SetActive(level == 0);
		m_otherFloor.SetActive(level > 0);
		transform.SetLocalY(3f * level);
		
		RoomData[] roomsData = {
			PlayerPrefsManager.UserData.floors[level].waitingRoom,
			PlayerPrefsManager.UserData.floors[level].vaultRoom,
			PlayerPrefsManager.UserData.floors[level].callCenterRoom,
			PlayerPrefsManager.UserData.floors[level].breakRoom,
		};
		roomsData[0].Init(Settings.Instance.company.waitingRoom, level);
		roomsData[1].Init(Settings.Instance.company.vaultRoom, level);
		roomsData[2].Init(Settings.Instance.company.callCenterRoom, level);
		roomsData[3].Init(Settings.Instance.company.breakRoom, level);
		for (int i = 0; i < rooms.Length; i++) {
			int ii = i;
			rooms[i].Init(roomsData[i], () => {
				onRoomTap(rooms[ii]);
			});
		}
	}
	public void SetRoomsGraphic() {
		for (int i = 0; i < rooms.Length; i++) {
			rooms[i].SetRoomGraphic();
		}
	}

	public void ClearRoomsGraphic() {
		for (int i = 0; i < rooms.Length; i++) {
			rooms[i].ClearRoomGraphic();
		}
	}

	public void PlayParticles() {
		GameObject particles = Instantiate(upgradeParticles, upgradeParticles.transform.parent);
		particles.SetActive(true);
		Destroy(particles.gameObject, 1.7f);
	}

	public void UpdateUpgradeObjects() {
		for (int i = 0; i < rooms.Length; i++) {
			rooms[i].UpdateUpgradeObject();
		}
	}

	public void UpdateUITextScales(Transform cameraTransform) {
		for (int i = 0; i < rooms.Length; i++) {
			rooms[i].UpdateUITextScale(cameraTransform);
		}
	}
}

[Serializable]
public class FloorData {
	public RoomData waitingRoom = new();
	public VaultRoomData vaultRoom = new();
	public RoomData callCenterRoom = new();
	public RoomData breakRoom = new();
}
