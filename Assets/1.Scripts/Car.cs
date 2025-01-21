using UnityEngine;
using ArcadeVP;

public class Car : MonoBehaviour {

	[SerializeField] private RoadLane roadLane;
	[SerializeField] private ArcadeVehicleController arcadeVehicleController;
	
	private void Update() {
		arcadeVehicleController.ProvideInputs(0f, 1f, 0f);
	}
}