using UnityEngine;

public class CompanyController : MonoBehaviour {
	
	[SerializeField] private StartSegment startSegment;
	[SerializeField] private ParkingController parkingController;
	[SerializeField] private VaultRoom vaultRoom;

	public void Init() {
		Activate();
		
		vaultRoom.Init(PlayerPrefsManager.UserData.vaultRoom, () => {
			
		});
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
