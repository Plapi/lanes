using UnityEngine;

public class GameScenario : MonoBehaviour {

	[SerializeField] private AICar aiCar;
	[SerializeField] private AICarPath[] paths;
	
	private void Start() {
		Application.targetFrameRate = 30;
		SetPath();
	}

	private void SetPath(int index = 0) {
		if (index >= paths.Length) {
			index = 0;
		}
		paths[index].SetCar(aiCar, () => {
			SetPath(index + 1);
		}, () => true);
	}
}