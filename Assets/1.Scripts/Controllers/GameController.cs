using System;
using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviour {

	public static GameController Instance { get; private set; }

	[SerializeField] private CameraController cameraController;
	[SerializeField] private CompanyController companyController;
	[SerializeField] private RideController rideController;
	[SerializeField] private GameObject leftEnvBuildings;
	
	private UIMainPanel mainPanel;
	private UIRoomPanel roomPanel;
	private UIParkingRoomPanel parkingRoomPanel;
	private UIShopPanel shopPanel;
	private UIDriversPanel driversPanel;
	private UIWatchAdPanel watchAdPanel;
	private UIIncomePanel incomePanel;
	private UIWelcomeBackPanel welcomeBackPanel;
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
		
		cameraController.SetHeight(GetFloorHeight(PlayerPrefsManager.UserData.GetFlorReached()));
		companyController.Init(cameraController.Camera.transform, OnRoomTap);
		rideController.Init(OnCompanyButton);
		
		TrackGenerator.Instance.OnStartSegmentSetActive += active => {
			companyController.gameObject.SetActive(active);
		};

		if (PlayerPrefsManager.UserData.TryGetTotalIncomeFromLastCollect(out int income, out int seconds)) {
			TryShowWelcomeBackPanel(income, seconds);
		}
		PlayerPrefsManager.UserData.lastCollectTime = new SerializedDateTime(DateTime.Now);
		if (income > 0) {
			PlayerPrefsManager.UserData.IncreaseCoins(income);
		}
		StartCoroutine(CoinsMechanic());
	}

	private void OnRoomTap(Room room) {
		cameraController.SetEnabled(false);
		cameraController.Zoom(room.GetCameraZoom());
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
	}

	private void UpgradeRoom(Room room) {
		PlayerPrefsManager.UserData.coins -= room.RoomData.UpgradeCost;
		room.RoomData.level++;
		if (room is ParkingRoom parkingRoom) {
			parkingRoom.RoomData.UnlockNewSlot();
		}
		PlayerPrefsManager.SaveUserData();
		room.SetRoomGraphic();
		room.PlayParticles();
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
		companyController.DriversController.OnHireFireDriver(driver);
	}

	private void OnCloseFromRoomPanel() {
		cameraController.SetEnabled(true);
		cameraController.ZoomBack();
	}

	private void InitUI() {
		UIController.Instance.Init();
		mainPanel = UIController.Instance.GetPanel<UIMainPanel>();
		roomPanel = UIController.Instance.GetPanel<UIRoomPanel>();
		parkingRoomPanel = UIController.Instance.GetPanel<UIParkingRoomPanel>();
		shopPanel = UIController.Instance.GetPanel<UIShopPanel>();
		driversPanel = UIController.Instance.GetPanel<UIDriversPanel>();
		watchAdPanel = UIController.Instance.GetPanel<UIWatchAdPanel>();
		incomePanel = UIController.Instance.GetPanel<UIIncomePanel>();
		settingsPanel = UIController.Instance.GetPanel<UISettingsPanel>();
		welcomeBackPanel = UIController.Instance.GetPanel<UIWelcomeBackPanel>();
		
		mainPanel.Init(new UIMainPanel.Data {
			onCoinsButton = () => {
				incomePanel.Show();
				incomePanel.Init(new UIIncomePanel.Data());
			},
			onShopButton = () => {
				shopPanel.Show();
				shopPanel.Init(new UIShopPanel.Data {
					addCoins = coins => {
						PlayerPrefsManager.UserData.IncreaseCoins(coins, false);
						mainPanel.CoinsPanel.UpdateCoins(PlayerPrefsManager.UserData.coins);
						mainPanel.CoinsPanel.PlayReceiveCoinsAnim();
					}
				});
			},
			onDriversButton = () => {
				driversPanel.Show();
				driversPanel.Init(GetDriversPanelData());
			},
			onMultiplyCashButton = () => {
				watchAdPanel.Show();
				watchAdPanel.Init(new UIWatchAdPanel.Data());
			},
			onDriveButton = OnDriveButton,
			onSettingsButton = settingsPanel.Show 
		});
		
		mainPanel.FloorPanel.Init(PlayerPrefsManager.UserData.GetFlorReached(), floor => {
			companyController.UpdateFloorLevel(floor);
			companyController.UpdateFloorGraphic(floor);
			cameraController.UpdateHeight(GetFloorHeight(Mathf.Min(PlayerPrefsManager.UserData.GetFlorReached(), floor)));
		}, UpgradeFloorStart, UpgradeFloorComplete);
		
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

	private void UpgradeFloorStart() {
		PlayerPrefsManager.UserData.StartUpgradeFloor();
		mainPanel.CoinsPanel.ConsumeCoins(PlayerPrefsManager.UserData.coins);
		OnCoinsUpdate?.Invoke();
		if (PlayerPrefsManager.UserData.removeAdsPurchased) {
			UpgradeFloorComplete();
		}
	}

	private void UpgradeFloorComplete() {
		PlayerPrefsManager.UserData.UpgradeFloor();
		companyController.UpgradeFloor(OnRoomTap);
		companyController.UpdateVaultTables(PlayerPrefsManager.UserData.coins);
		cameraController.UpdateHeight(GetFloorHeight(PlayerPrefsManager.UserData.GetFlorReached()));
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
			leftEnvBuildings.gameObject.SetActive(true);
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
			leftEnvBuildings.gameObject.SetActive(false);
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

	private void TryShowWelcomeBackPanel(int income, int seconds) {
		if (income < 500 || seconds < 180) {
			return;
		}
		UIController.Instance.ActivateTouchBlocker(1f);
		this.Wait(1f, () => {
			welcomeBackPanel.Show();
			welcomeBackPanel.Init(new UIWelcomeBackPanel.Data {
				income = income,
				seconds = seconds,
				onWatchAd = () => {
					AdsController.Instance.ShowAd(success => {
						if (success) {
							PlayerPrefsManager.UserData.IncreaseCoins(income, false);
							mainPanel.CoinsPanel.UpdateCoins(PlayerPrefsManager.UserData.coins);
							mainPanel.CoinsPanel.PlayReceiveCoinsAnim();
						}
					});
				}
			});
		});
	}

	private IEnumerator CoinsMechanic() {
		PlayerPrefsManager.UserData.TryGetCoinsIncome(out int income);
		UpdateCoins(income);
		
		while (true) {
			bool hasIncome = PlayerPrefsManager.UserData.TryGetCoinsIncome(out income);
			while (!hasIncome) {
				yield return new WaitForSeconds(2f);
				hasIncome = PlayerPrefsManager.UserData.TryGetCoinsIncome(out income);
				UpdateCoins(income);
			}
			
			while (PlayerPrefsManager.UserData.VaultIsFull()) {
				yield return new WaitForSeconds(2f);
				PlayerPrefsManager.UserData.TryGetCoinsIncome(out income);
				UpdateCoins(income);
			}

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(Settings.Instance.company.incomeTurnDuration);
			DateTime now = startTime;

			PlayerPrefsManager.UserData.lastCollectTime = new SerializedDateTime(DateTime.Now);
			PlayerPrefsManager.SaveUserData();
			
			while (now < endTime) {
				yield return null;
				yield return new WaitUntil(() => mainPanel.CoinsPanel.gameObject.activeSelf && mainPanel.CoinsPanel.gameObject.activeInHierarchy);
				now = DateTime.Now;
				mainPanel.CoinsPanel.UpdateProgress(Mathf.Min(1f, (float)(now - startTime).TotalSeconds / Settings.Instance.company.incomeTurnDuration));
			}

			while (PauseUpdateCoins) {
				yield return null;
			}
			
			if (PlayerPrefsManager.UserData.TryGetTotalIncomeFromLastCollect(out income, out int seconds)) {
				mainPanel.CoinsPanel.UpdateProgress(1f);
				PlayerPrefsManager.UserData.IncreaseCoins(income);
				mainPanel.CoinsPanel.PlayCoinsIncomeAnim(() => {
					if (this == null) {
						return;
					}
					UpdateCoins(income);
					OnCoinsUpdate?.Invoke();
				});
				TryShowWelcomeBackPanel(income, seconds);
			}
		}
	}

	public bool PauseUpdateCoins;
	private void UpdateCoins(int income) {
		if (PauseUpdateCoins) {
			return;
		}
		int coins = PlayerPrefsManager.UserData.coins;
		mainPanel.CoinsPanel.UpdateCoins(coins, income);
		companyController.UpdateVaultTables(coins);
		companyController.UpdateUpgradeObjects();
		mainPanel.UpdateBoostIncomeObjects();
	}

	private void Update() {
		companyController.UpdateVisibility(visible => {
			mainPanel.FloorPanel.UpdateVisibility(!visible);
			if (!visible) {
				cameraController.SetHeight(GetFloorHeight(PlayerPrefsManager.UserData.GetFlorReached()));
			}
		});
	}

	private static float GetFloorHeight(int level) {
		return Settings.Instance.company.floorHeight * level;
	}
}
