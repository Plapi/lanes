using UnityEngine;

public class TrafficLight : Element {

	[SerializeField] private GameObject redOff;
	[SerializeField] private GameObject yellowOff;
	[SerializeField] private GameObject greenOff;

	public void SetRed() {
		ActivateAll();
		redOff.SetActive(false);
	}

	public void SetYellow() {
		ActivateAll();
		yellowOff.SetActive(false);
	}

	public void SetGreen() {
		ActivateAll();
		greenOff.SetActive(false);
	}

	private void ActivateAll() {
		redOff.SetActive(true);
		yellowOff.SetActive(true);
		greenOff.SetActive(true);
	}
}
