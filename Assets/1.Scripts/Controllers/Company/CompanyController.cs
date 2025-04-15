using UnityEngine;

public class CompanyController : MonoBehaviour {

	[SerializeField] private StartSegment segment;
	[SerializeField] private Parking[] parkings;
	
	private void Awake() {
		segment.Init(Segment.GetSegmentData(new SegmentInputData { length = 200 }));
		segment.SetStartAndEndPosForRoadLanes();
		for (int i = 0; i < parkings.Length; i++) {
			parkings[i].Init(segment.RoadLanes[^1]);
			if (Utils.CoinFlip()) {
				parkings[i].SetCar();
			}
		}
		segment.SpawnAICars();
		TravelCar();
	}

	private void TravelCar() {
		Parking parking = parkings[Random.Range(0, parkings.Length)];
		if (parking.HasCar()) {
			parking.ExitCar(TravelCar);
		} else {
			parking.EnterCar(TravelCar);
		}
	}
}
