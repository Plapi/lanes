using UnityEngine;

public class CompanyController : MonoBehaviour {
	
	[SerializeField] private StartSegment startSegment;
	[SerializeField] private ParkingController parkingController;
	
	private void Awake() {
		startSegment.Init(Segment.GetSegmentData(new SegmentInputData { length = 200 }));
		startSegment.SetStartAndEndPosForRoadLanes();
		parkingController.Init(startSegment);
		startSegment.SpawnAICars();
	}
}
