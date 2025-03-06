using System;
using UnityEngine;

public class PersonPickupController : MonoBehaviour {
    
	[SerializeField] private Person person;
	[SerializeField] private Transform startPin;
	[SerializeField] private Transform endPin;

	private UserCar userCar;
	public PickupState State { get; private set; }
	public Action OnPickup;
	public Action OnDrop;
	public Action OnMiss;
	public Action<int> OnUpdateDistance;
	
	public void SetPickUp(Vector3 pos, UserCar userCar) {
		person.SetWaving();
		person.transform.position = pos;
		startPin.gameObject.SetActive(true);
		startPin.transform.position = new Vector3(pos.x - 3.5f, 0f, pos.z);
		this.userCar = userCar;
		State = PickupState.WaitingForPickup;
	}

	public void SetEndPin(Vector3 pos) {
		person.transform.position = pos;
		person.transform.SetZ(pos.z + 5f);
		endPin.gameObject.SetActive(true);
		endPin.transform.position = new Vector3(pos.x - 3.5f, 0f, pos.z);
	}

	private void Update() {
		if (State == PickupState.WaitingForPickup) {
			person.transform.LookAt(userCar.transform);
			if (Vector3.Distance(userCar.FrontPos, startPin.position) < 2f) {
				State = PickupState.Pickup;
				person.gameObject.SetActive(false);
				startPin.gameObject.SetActive(false);
				OnPickup?.Invoke();
				return;
			}
			if (userCar.FrontPos.z > startPin.position.z + 20f) {
				State = PickupState.None;
				person.gameObject.SetActive(false);
				startPin.gameObject.SetActive(false);
			}
		} else if (State == PickupState.Pickup) {
			if (endPin.gameObject.activeSelf) {
				int distance = Mathf.RoundToInt(Vector3.Distance(userCar.FrontPos, endPin.position));
				OnUpdateDistance?.Invoke(distance);
				if (distance < 2f) {
					State = PickupState.Finish;
					endPin.gameObject.SetActive(false);
					person.SetThankful();
					OnDrop?.Invoke();
					return;
				}
				if (userCar.FrontPos.z > endPin.position.z + 5f) {
					State = PickupState.None;
					endPin.gameObject.SetActive(false);
					OnMiss?.Invoke();
				}				
			}
		} else if (State == PickupState.Finish) {
			if (userCar.FrontPos.z > endPin.position.z + 20f) {
				State = PickupState.None;
				endPin.gameObject.SetActive(false);
				person.gameObject.SetActive(false);
			}
		}
	}
}

public enum PickupState {
	None,
	WaitingForPickup,
	Pickup,
	Finish
}