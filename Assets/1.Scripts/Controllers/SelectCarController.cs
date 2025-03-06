using System;
using UnityEngine;
using ArcadeVP;

public class SelectCarController : MonoBehaviour {

	[SerializeField] private RotateObjController rotateObjController;
	[SerializeField] private UserCar[] userCars;

	private Transform[] templatesUserCar;
	private Transform templatesContainer;

	private UIGaragePanel garagePanel;
	
	private int selection;
	
	public void Init() {
		
		garagePanel = UIController.Instance.GetPanel<UIGaragePanel>();
		
		selection = PlayerPrefsManager.UserData.carSelection;
		
		for (int i = 0; i < userCars.Length; i++) {
			userCars[i].gameObject.SetActive(false);
		}
		
		templatesUserCar = new Transform[userCars.Length];
		
		templatesContainer = new GameObject("templates").transform;
		templatesContainer.parent = transform;
		templatesContainer.SetLocalPositionAndRotation(userCars[0].transform.position, userCars[0].transform.rotation);
		templatesContainer.SetSiblingIndex(0);
		
		for (int i = 0; i < templatesUserCar.Length; i++) {
			templatesUserCar[i] = Instantiate(userCars[i], templatesContainer).transform;
			templatesUserCar[i].SetPositionAndRotation(userCars[i].transform.position, userCars[i].transform.rotation);
			templatesUserCar[i].name = templatesUserCar[i].name.Replace("(Clone)", string.Empty);
			templatesUserCar[i].gameObject.SetActive(false);
			RemoveComponentsInChildren(templatesUserCar[i], typeof(UserCar), 
				typeof(ArcadeVehicleController), typeof(SphereCollider), typeof(HingeJoint), typeof(Rigidbody),
				typeof(AudioSource));
			Destroy(templatesUserCar[i].GetChild(2).gameObject);
			SkidMarks[] skidMarks = templatesUserCar[i].GetComponentsInChildren<SkidMarks>();
			for (int j = 0; j < skidMarks.Length; j++) {
				Destroy(skidMarks[j].gameObject);
			}
		}

		UpdateSelection(0);
	}

	public void UpdateSelection(int add) {
		templatesUserCar[selection].gameObject.SetActive(false);
		templatesUserCar[selection].SetPositionAndRotation(userCars[selection].transform.position, userCars[selection].transform.rotation);
		selection = Mathf.Clamp(selection + add, 0, userCars.Length - 1);
		templatesUserCar[selection].gameObject.SetActive(true);

		bool carIsUnlocked = CarIsUnlocked(selection);
		
		garagePanel.SetLeftRightButtonInteractable(selection > 0, selection < userCars.Length - 1);
		garagePanel.UpdateBottom(CarIsUnlocked(selection) ? -1 : userCars[selection].Price);
		rotateObjController.SetObj(carIsUnlocked ? templatesUserCar[selection].GetComponent<BoxCollider>() : null);
	}

	public void BuyCar() {
		int coins = PlayerPrefsManager.UserData.coins;
		if (coins >= userCars[selection].Price) {
			PlayerPrefsManager.UserData.coins -= userCars[selection].Price;
			PlayerPrefsManager.UserData.unlockedCars.Add(selection);
			PlayerPrefsManager.SaveUserData();
			garagePanel.UpdateCoins(PlayerPrefsManager.UserData.coins);
			garagePanel.UpdateBottom(0);
			rotateObjController.SetObj(templatesUserCar[selection].GetComponent<BoxCollider>());
		} else {
			UIController.Instance.GetPanel<UIInfoPanel>().Init(new UIInfoPanel.Data {
				title = "Not Enough Coins!",
				description = "Not enough coins for this car! Earn more by picking up passengers and completing rides."
			}).Show();
		}
	}

	public UserCar GetUserCarAndGo() {
		PlayerPrefsManager.UserData.carSelection = selection;
		PlayerPrefsManager.SaveUserData();
		
		templatesUserCar[selection].SetPositionAndRotation(userCars[selection].transform.position, userCars[selection].transform.rotation);
		
		templatesContainer.gameObject.SetActive(false);
		userCars[selection].gameObject.SetActive(true);
		rotateObjController.enabled = false;
		
		return userCars[selection];
	}

	public void ReInit() {
		templatesContainer.gameObject.SetActive(true);
		rotateObjController.enabled = true;
	}
	
	private static bool CarIsUnlocked(int selection) {
		return PlayerPrefsManager.UserData.unlockedCars.Contains(selection);
	}
	
	private static void RemoveComponentsInChildren(Transform root, params Type[] componentTypes) {
		foreach (Type componentType in componentTypes) {
			Component[] components = root.GetComponentsInChildren(componentType, true);
			foreach (Component component in components) {
				Destroy(component);
			}
		}
	}
}