using UnityEngine;

public class GameScenario : MonoBehaviour {

	[SerializeField] private UserCar userCar;
	[SerializeField] private AICar aiCar;
	
	private void Update() {
		userCar.UpdateCar(1f, 0.5f);
		aiCar.SetTargetPoint(new TargetPoint {
			pos = aiCar.transform.position + aiCar.transform.forward * 10f
		});
	}

}