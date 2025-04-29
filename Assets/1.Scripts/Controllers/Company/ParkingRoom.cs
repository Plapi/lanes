using System;
using System.Collections.Generic;
using UnityEngine;

public class ParkingRoom : Room {
    
	[SerializeField] private Parking[] parkingLots;
	
	public new ParkingRoomData RoomData => (ParkingRoomData)base.RoomData;
	
	public void Activate(Segment segment) {
		for (int i = 0; i < parkingLots.Length; i++) {
			parkingLots[i].Init(segment.RoadLanes[^1]);
			if (RoomData.parkingSlots[i].taxiPurchased && (!RoomData.parkingSlots[i].HasDriver || Utils.CoinFlip())) {
				parkingLots[i].SetCar();
			}
		}
		TravelCar();
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

	private void TravelCar() {
		if (!TryGetRandomParkingLot(out Parking parking)) {
			this.Wait(2f, TravelCar);
			return;
		}
		if (parking.HasCar()) {
			parking.ExitCar(TravelCar);
		} else {
			parking.EnterCar(TravelCar);
		}
	}

	private bool TryGetRandomParkingLot(out Parking parking) {
		List<Parking> parkingLotsWithTaxi = new();
		List<ParkingSlotData> parkingSlotsData = new();
		for (int i = 0; i < RoomData.parkingSlots.Length; i++) {
			if (RoomData.parkingSlots[i].taxiPurchased) {
				parkingLotsWithTaxi.Add(parkingLots[i]);
				parkingSlotsData.Add(RoomData.parkingSlots[i]);
			}
		}
		parking = null;
		int randomIndex = 0;
		do {
			if (parking != null) {
				parkingLotsWithTaxi.RemoveAt(randomIndex);
				parkingSlotsData.RemoveAt(randomIndex);
				parking = null;
			}
			if (parkingLotsWithTaxi.Count == 0) {
				break;
			}
			randomIndex = UnityEngine.Random.Range(0, parkingLotsWithTaxi.Count);
			parking = parkingLotsWithTaxi[randomIndex];
		} while (parking.HasCar() && !parkingSlotsData[randomIndex].HasDriver);
		return parking != null;
	}
}

[Serializable]
public class ParkingSlotData {
	public bool slotUnlocked;
	public bool taxiPurchased;
	public string driverId;
	
	public bool HasDriver => !string.IsNullOrEmpty(driverId);
}
