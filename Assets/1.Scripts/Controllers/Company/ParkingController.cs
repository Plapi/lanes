using UnityEngine;

public class ParkingController : MonoBehaviour {
	
	[SerializeField] private Parking[] parkingLots;

	public void Activate(Segment segment) {
		for (int i = 0; i < parkingLots.Length; i++) {
			parkingLots[i].Init(segment.RoadLanes[^1]);
			if (Utils.CoinFlip()) {
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

	private void TravelCar() {
		Parking parking = parkingLots[Random.Range(0, parkingLots.Length)];
		if (parking.HasCar()) {
			parking.ExitCar(TravelCar);
		} else {
			parking.EnterCar(TravelCar);
		}
	}
	
}
