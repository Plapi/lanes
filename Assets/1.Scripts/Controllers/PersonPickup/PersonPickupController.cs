using System;
using UnityEngine;

public class PersonPickupController : MonoBehaviour {
    
	[SerializeField] private Person person;
	[SerializeField] private Transform startPin;
	[SerializeField] private Transform endPin;

	private UserCar userCar;
	
	public PickupState State { get; private set; }
	public Action OnPickup;
	public Action OnNotPickup;
	public Action OnDrop;
	public Action OnDropMissed;
	public Action<int> OnUpdateDistance;
	
	public void SetPickUp(Vector3 pos, Transform segment, UserCar userCar, int personIndex) {
		person.SetWaving(personIndex);
		person.transform.position = pos;
		startPin.transform.position = pos - segment.transform.right * 3.5f;
		startPin.transform.forward = segment.transform.forward;
		startPin.gameObject.SetActive(true);
		this.userCar = userCar;
		State = PickupState.WaitingForPickup;
	}

	public void SetEndPin(Vector3 pos, Transform segment) {
		person.transform.position = pos + segment.transform.forward * 5f;
		endPin.transform.position = pos - segment.transform.right * 3.5f;
		endPin.transform.forward = segment.transform.forward;
		endPin.gameObject.SetActive(true);
	}

	private void Update() {
		if (person.gameObject.activeSelf && userCar != null) {
			person.transform.LookAt(userCar.transform);
		}
		if (State == PickupState.WaitingForPickup) {
			int distance = Mathf.RoundToInt(Vector3.Distance(userCar.FrontPos, startPin.position));
			if (distance < 2f) {
				State = PickupState.Pickup;
				person.gameObject.SetActive(false);
				startPin.gameObject.SetActive(false);
				OnPickup?.Invoke();
				return;
			}
			if (distance < 50f &&
			    Vector3.Dot(userCar.transform.forward, (startPin.position - userCar.transform.position).normalized) < 0f) {
				State = PickupState.None;
				person.gameObject.SetActive(false);
				startPin.gameObject.SetActive(false);
				OnNotPickup?.Invoke();
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
				if (distance < 50f &&
					Vector3.Dot(userCar.transform.forward, (endPin.position - userCar.transform.position).normalized) < 0f) {
					State = PickupState.None;
					endPin.gameObject.SetActive(false);
					OnDropMissed?.Invoke();
				}
			}
		} else if (State == PickupState.Finish) {
			int distance = Mathf.RoundToInt(Vector3.Distance(userCar.FrontPos, endPin.position));
			if (distance > 20f) {
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