using System;
using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviour {

	public static GameController Instance { get; private set; }

	[SerializeField] private CameraController cameraController;
	[SerializeField] private CompanyController companyController;
	[SerializeField] private RideController rideController;
	
	private UIMainPanel mainPanel;
	private UIRoomPanel roomPanel;
	private UIParkingRoomPanel parkingRoomPanel;
	private UIDriversPanel driversPanel;
	private UISettingsPanel settingsPanel;

	public Action OnCoinsUpdate;

	private void Awake() {
		Instance = this;
	}
	
	private void Start() {
		Application.targetFrameRate = 60;
		if (Settings.Instance.testMode) {
			gameObject.AddComponent<DebugFPS>();
		}
		
		AudioSystem.Init(this, PlayerPrefsManager.UserData.volumes);
		HapticFeedback.SetEnabled(PlayerPrefsManager.UserData.hapticFeedback);
		
		InitUI();
		
		companyController.Init(room => {
			cameraController.SetEnabled(false);
			cameraController.Zoom(room.GetCameraZoom());
			mainPanel.ShowSettingsButton(false);
			if (room is ParkingRoom) {
				parkingRoomPanel.Show();
				parkingRoomPanel.Init(new UIParkingRoomPanel.Data {
					roomData = (ParkingRoomData)room.RoomData,
					onClose = OnCloseFromRoomPanel,
					onUpgrade = () => UpgradeRoom(room),
					onBuyTaxi = BuyTaxi,
					onAssignDriver = OnAssignDriver
				});
			} else {
				roomPanel.Show();
				roomPanel.Init(new UIRoomPanel.Data {
					roomData = room.RoomData,
					onClose = OnCloseFromRoomPanel,
					onUpgrade = () => UpgradeRoom(room),
				});
			}
		});
		rideController.Init(OnCompanyButton);
		
		TrackGenerator.Instance.OnStartSegmentSetActive += active => {
			companyController.gameObject.SetActive(active);
		};
		
		StartCoroutine(CoinsMechanic());
	}

	private void UpgradeRoom(Room room) {
		UIController.Instance.ActivateTouchBlocker(2f);
		PlayerPrefsManager.UserData.coins -= room.RoomData.UpgradeCost;
		room.RoomData.level++;
		if (room is ParkingRoom parkingRoom) {
			parkingRoom.RoomData.UnlockNewSlot();
		}
		PlayerPrefsManager.SaveUserData();
		room.UpdateRoomGraphic();
		PlayerPrefsManager.UserData.TryGetCoinsIncome(out int income);
		UpdateCoins(income);
		mainPanel.CoinsPanel.PlayConsumeCoinsAnim();
		OnCoinsUpdate?.Invoke();
	}

	private void BuyTaxi(ParkingSlotData parkingSlotData) {
		PlayerPrefsManager.UserData.coins -= Settings.Instance.company.parkingRoom.taxiCost;
		mainPanel.CoinsPanel.ConsumeCoins(PlayerPrefsManager.UserData.coins);
		parkingSlotData.taxiPurchased = true;
		PlayerPrefsManager.SaveUserData();
		companyController.ParkingRoom.SetCar(parkingSlotData);
		OnCoinsUpdate?.Invoke();
	}

	private void OnAssignDriver(ParkingSlotData parkingSlotData) {
		driversPanel.Show();
		UIDriversPanel.Data data = GetDriversPanelData();
		data.onSelect = driver => {
			driversPanel.Close();
			parkingSlotData.driverId = driver.design.id;
			PlayerPrefsManager.SaveUserData();
			PlayerPrefsManager.UserData.TryGetCoinsIncome(out int income);
			UpdateCoins(income);
		};
		driversPanel.Init(data);
	}

	private void HireFireDriver(DriverData driver) {
		driver.hired = !driver.hired;
		PlayerPrefsManager.UserData.coins += driver.hired ? -driver.design.hireCost : driver.design.fireCost;
		mainPanel.CoinsPanel.UpdateCoins(PlayerPrefsManager.UserData.coins);
		if (driver.hired) {
			mainPanel.CoinsPanel.PlayConsumeCoinsAnim();
		} else {
			mainPanel.CoinsPanel.PlayReceiveCoinsAnim();
			if (PlayerPrefsManager.UserData.TryGetParkingSlotIndex(driver, out int index)) {
				PlayerPrefsManager.UserData.parkingRoom.parkingSlots[index].driverId = null;
			}
		}
		PlayerPrefsManager.SaveUserData();
		OnCoinsUpdate?.Invoke();
	}

	private void OnCloseFromRoomPanel() {
		cameraController.SetEnabled(true);
		cameraController.ZoomBack();
		mainPanel.ShowSettingsButton(true);
	}

	private void InitUI() {
		UIController.Instance.Init();
		mainPanel = UIController.Instance.GetPanel<UIMainPanel>();
		roomPanel = UIController.Instance.GetPanel<UIRoomPanel>();
		parkingRoomPanel = UIController.Instance.GetPanel<UIParkingRoomPanel>();
		driversPanel = UIController.Instance.GetPanel<UIDriversPanel>();
		settingsPanel = UIController.Instance.GetPanel<UISettingsPanel>();
		
		mainPanel.Init(new UIMainPanel.Data {
			onSettingsButton = settingsPanel.Show, 
			onDriversButton = () => {
				driversPanel.Show();
				driversPanel.Init(GetDriversPanelData());
			},
			onMultiplyCashButton = () => {
				
			}, onDriveButton = OnDriveButton
		});
		
		settingsPanel.Init(new UISettingsPanel.Data {
			volumes = PlayerPrefsManager.UserData.volumes,
			onUpdateSlider = (index, volume) => {
				MixerType mixerType = (MixerType)index;
				if (mixerType == MixerType.CarEngine) {
					UserCar userCar = rideController.GetUserCar();
					if (userCar != null) {
						userCar.SetAudioVolume(volume);
					}
				} else {
					AudioSystem.UpdateVolume(mixerType, volume);	
				}
			},
			hapticFeedback = PlayerPrefsManager.UserData.hapticFeedback,
			onUpdateHapticFeedback = hapticFeedback => {
				PlayerPrefsManager.UserData.hapticFeedback = hapticFeedback;
				PlayerPrefsManager.SaveUserData();
				HapticFeedback.SetEnabled(hapticFeedback);
			}, onClose = volumes => {
				PlayerPrefsManager.UserData.volumes = volumes;
				PlayerPrefsManager.SaveUserData();
				AnalyticsSystem.RecordSettingsEvent(Mathf.RoundToInt(PlayerPrefsManager.UserData.volumes[0] * 100),
					Mathf.RoundToInt(PlayerPrefsManager.UserData.volumes[1] * 100),
					Mathf.RoundToInt(PlayerPrefsManager.UserData.volumes[2] * 100), true);
			}, onAbout = () => {
				AnalyticsSystem.RecordOpenAboutEvent();
				UIController.Instance.GetPanel<UIAboutPanel>().Init(new UIAboutPanel.Data {
					onMail = () => {
						AnalyticsSystem.RecordClickMailEvent();
						const string email = "adrian.plapamaru@gmail.com";
						string subject = Utils.EscapeURL("Feedback about Rade Empire");
						string body = Utils.EscapeURL("Hi, I’d like to share my thoughts about the game...");
						string mailto = $"mailto:{email}?subject={subject}&body={body}";
						Application.OpenURL(mailto);
					}
				}).Show();
			}
		});
	}

	private UIDriversPanel.Data GetDriversPanelData() {
		return new UIDriversPanel.Data {
			drivers = PlayerPrefsManager.UserData.drivers,
			onHire = HireFireDriver,
			onFire = HireFireDriver
		};
	}
	
	private void OnDriveButton() {
		UIController.Instance.FadeInToBlack(() => {
			cameraController.gameObject.SetActive(false);
			TrackGenerator.Instance.SetSpawnAICarDistance(90, 110, 20, 40);
			mainPanel.MoveToOtherPanel(UIController.Instance.GetPanel<UIGaragePanel>().TopContainer);
			mainPanel.Close();
			companyController.Deactivate();
			rideController.Activate();
			UIController.Instance.FadeOutFromBlack();
		});
	}

	private void OnCompanyButton() {
		UIController.Instance.FadeInToBlack(() => {
			cameraController.gameObject.SetActive(true);
			TrackGenerator.Instance.SetSpawnAICarDistance(40, 80, 40, 80);
			UIController.Instance.GetPanel<UIGaragePanel>().Close(false);
			mainPanel.MoveBack();
			mainPanel.Show();
			rideController.Deactivate();
			companyController.Activate();
			UIController.Instance.FadeOutFromBlack();
		});
	}

	private IEnumerator CoinsMechanic() {

		int income;
		while (!PlayerPrefsManager.UserData.TryGetCoinsIncome(out income)) {
			UpdateCoins(income);
			yield return new WaitForSeconds(2f);
		}

		if (PlayerPrefsManager.UserData.VaultIsFull()) {
			UpdateCoins(income);	
			do {
				yield return new WaitForSeconds(2f);
			} while (PlayerPrefsManager.UserData.VaultIsFull());
		}
		UpdateCoins(income);
		
		const float time = 10f;
		float rTime = 0f;
		while (rTime < time) {
			yield return null;
			rTime += Time.deltaTime;
			mainPanel.CoinsPanel.UpdateProgress(rTime / time);
		}
		
		DateTime startTime = DateTime.Now;
		yield return new WaitUntil(() => mainPanel.CoinsPanel.gameObject.activeSelf && mainPanel.CoinsPanel.gameObject.activeInHierarchy);
		float seconds = (float)(DateTime.Now - startTime).TotalSeconds;
		int turns = Mathf.RoundToInt(seconds / 12);
		PlayerPrefsManager.UserData.TryGetCoinsIncome(out income);
		income += turns * income;
		
		mainPanel.CoinsPanel.UpdateProgress(1f);
		PlayerPrefsManager.UserData.IncreaseCoins(income);
		OnCoinsUpdate?.Invoke();
		
		mainPanel.CoinsPanel.PlayCoinsIncomeAnim(() => StartCoroutine(CoinsMechanic()));
	}

	private void UpdateCoins(int income) {
		int coins = PlayerPrefsManager.UserData.coins;
		mainPanel.CoinsPanel.UpdateCoins(coins, income);
		companyController.VaultRoom.UpdateTables(coins);
	}
}
