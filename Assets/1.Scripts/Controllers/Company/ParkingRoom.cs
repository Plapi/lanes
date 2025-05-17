using System;
using System.Collections.Generic;
using UnityEngine;

public class ParkingRoom : Room {
    
	[SerializeField] private Parking[] parkingLots;
	
	public new ParkingRoomData RoomData => (ParkingRoomData)base.RoomData;
	
	public void Activate(Segment segment) {
		for (int i = 0; i < parkingLots.Length; i++) {
			parkingLots[i].Init(segment.RoadLanes[^1]);
			if (RoomData.parkingSlots[i].taxiPurchased) {
				parkingLots[i].SetCar();
			}
		}
	}

	public void Deactivate() {
		for (int i = 0; i < parkingLots.Length; i++) {
			parkingLots[i].ReleaseCarIfNeeded();
		}
	}

	public void SetCar(ParkingSlotData parkingSlotData) {
		for (int i = 0; i < parkingLots.Length; i++) {
			if (RoomData.parkingSlots[i] == parkingSlotData) {
				parkingLots[i].SetCar();
			}
		}
	}
	
	public bool TryGetParkingForEnter(out int parkingSlotIndex) {
		parkingSlotIndex = -1;
		List<int> eligibleParkingLots = new();
		for (int i = 0; i < parkingLots.Length; i++) {
			if (RoomData.parkingSlots[i].taxiPurchased && !parkingLots[i].HasCar()) {
				eligibleParkingLots.Add(i);
			}
		}
		if (eligibleParkingLots.Count == 0) {
			return false;
		}
		parkingSlotIndex = eligibleParkingLots[UnityEngine.Random.Range(0, eligibleParkingLots.Count)];
		return true;
	}

	public void EnterCar(int index, Action onComplete) {
		parkingLots[index].EnterCar(onComplete);
	}

	public void ExitCar(int index, Action onComplete) {
		parkingLots[index].ExitCar(onComplete);
	}
}

[Serializable]
public class ParkingSlotData {
	public bool slotUnlocked;
	public bool taxiPurchased;
	public string driverId;
	
	public bool HasDriver => !string.IsNullOrEmpty(driverId);
}
