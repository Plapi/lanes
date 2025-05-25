using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class RideController : MonoBehaviourSingleton<RideController> {

	[SerializeField] private new Camera camera;
	[SerializeField] private InputManager inputManager;
	[SerializeField] private SelectCarController selectCarController;
	[SerializeField] private RotateObjController rotateObjController;
	[SerializeField] private TrackGenerator trackGenerator;
	[SerializeField] private GameObject garage;
	[SerializeField] private Transform skyline;
	[SerializeField] private GameObject smoke;
	
	[Space]
	[SerializeField] private UIDrivingInput drivingInput;
	
	[Space]
	[SerializeField] private PersonPickupController personPickupController;
	[SerializeField] private bool personsEnabled;
	
	[Space]
	[SerializeField] private AudioClip[] onLoseHealthClips;
	
	private UserCar userCar;
	
	private bool canControlUserCar;
	
	private UIRideTopPanel rideTopPanel;
	private UIGaragePanel garagePanel;
	private UIPausePanel pausePanel;
	private UIResultsPanel resultsPanel;
	private UIMissionResultsPanel missionResultsPanel;
	private UITakeMissionPanel takeMissionPanel;
	
	private PosAndRot initCameraPosAndRot;
	private PosAndRot initUserCarPosAndRot;

	private int totalDistance;
	private int personPickupSegments;
	private readonly List<CurrentPerson> personsDropped = new();
	private int coinsEarned;

	private readonly List<GenerateDir> generatedDirs = new();
	private int currentGeneratedDirsIndex;

	private CurrentPerson currentPerson;
	
	private List<UIMissionsList.ItemData> missionsList;
	private UIMissionsList.ItemData selectedMission;

	private List<int> personPickupSegmentsList;

	protected override void Awake() {
		base.Awake();
		personPickupSegmentsList = new List<int> { 1, Random.Range(1, 3), Random.Range(2, 4) };
	}
	
	public void Init(Action onClose) {
		initCameraPosAndRot = new PosAndRot(camera.transform); 
		if (personsEnabled) { 
			InitPersonPickupController(); 
		}
		InitUI(onClose); 
		selectCarController.Init();
		trackGenerator.OnStartSegmentSetActive += active => {
			garage.SetActive(active);
		};
	}
	
	public void Activate() {
		camera.gameObject.SetActive(true);
		garagePanel.Show();
		trackGenerator.Init(GenerateDir.Forward);
		rotateObjController.enabled = true;
		GenerateTakeMissionsItems();
	}

	public void Deactivate() {
		camera.gameObject.SetActive(false);
		trackGenerator.ClearAllSegments();
		rotateObjController.enabled = false;
	}

	private void InitPersonPickupController() {
		int startDistance = 0;
		personPickupController.OnPickup = () => {
			if (personPickupSegmentsList.Count > 0) {
				personPickupSegments = personPickupSegmentsList[0];
				personPickupSegmentsList.RemoveAt(0);
			} else {
				personPickupSegments = Random.Range(1, 6);	
			}
			rideTopPanel.ShowPerson(personPickupSegments, currentPerson.GetSprite());
			startDistance = totalDistance;
			
			rideTopPanel.Navigation.UpdateCurrentPersonState(UINavigationItem.PersonData.State.Checkmark);
			if (personPickupSegments > 1) {
				rideTopPanel.Navigation.NextItem(new UINavigationItem.DirData { index = (int)GetNextGeneratedDir() });
			} else {
				rideTopPanel.Navigation.NextItem(new UINavigationItem.PersonData {
					group = currentPerson.group,
					index = currentPerson.index,
					state = UINavigationItem.PersonData.State.DropOff
				});
			}
		};
		personPickupController.OnNotPickup = () => {
			rideTopPanel.Navigation.UpdateCurrentPersonState(UINavigationItem.PersonData.State.Missed);
			if (selectedMission != null) {
				rideTopPanel.Navigation.NextItem(new UINavigationItem.PersonData {
					group = currentPerson.group,
					index = currentPerson.index,
				});
			} else {
				GenerateNewPerson();
			}
		};
		personPickupController.OnDrop = () => {
			personsDropped.Add(currentPerson);
			float distance = totalDistance - startDistance + Vector3.Distance(trackGenerator.GetNextSegment(generatedDirs[0]).transform.position, userCar.transform.position);
			int coins = Mathf.RoundToInt(userCar.CoinsMultiplier * Mathf.Lerp(50, 500, Mathf.InverseLerp(100f, 2500f, distance)));
			rideTopPanel.HidePerson(coins);
			coinsEarned += coins;
			rideTopPanel.Navigation.UpdateCurrentPersonState(UINavigationItem.PersonData.State.Checkmark);
			if (selectedMission != null) {
				userCar.SetSoundEnabled(false);
				canControlUserCar = false;
				drivingInput.gameObject.SetActive(false);
				ShowMissionResultsPanel();
			} else {
				GenerateNewPerson();
			}
		};
		personPickupController.OnDropMissed = () => {
			if (selectedMission != null) {
				rideTopPanel.HideDistance();
				rideTopPanel.Navigation.UpdateCurrentPersonState(UINavigationItem.PersonData.State.Missed);
				rideTopPanel.Navigation.NextItem(new UINavigationItem.PersonData {
					group = currentPerson.group,
					index = currentPerson.index,
					state = UINavigationItem.PersonData.State.DropOff
				});
			} else {
				rideTopPanel.HidePerson(-1);
				rideTopPanel.Navigation.UpdateCurrentPersonState(UINavigationItem.PersonData.State.Missed);
				GenerateNewPerson();
			}
		};
		personPickupController.OnUpdateDistance = distance => {
			rideTopPanel.ShowDistance(distance);
		};
	}

	private void GenerateNewPerson() {
		SetRandomCurrentPersonIndex();
		rideTopPanel.Navigation.NextItem(new UINavigationItem.PersonData {
			group = currentPerson.group,
			index = currentPerson.index,
		});
	}

	private IEnumerator OnUserCarEnd() {
		smoke.transform.parent = userCar.transform;
		smoke.transform.position = userCar.FrontPos;
		smoke.gameObject.SetActive(true);
		userCar.UpdateCar(0f, 0.5f);
		canControlUserCar = false;
		drivingInput.gameObject.SetActive(false);
		
		float time = 4f;
		while (time > 0f && !Input.GetMouseButtonDown(0)) {
			yield return null;
			time -= Time.deltaTime;
		}
		
		ShowResults();
	}

	private void ShowResults() {
		bool distanceBest = totalDistance > PlayerPrefsManager.UserData.distanceBest;
		bool personBest = personsDropped.Count > PlayerPrefsManager.UserData.personsBest;
		Time.timeScale = 0f;
		resultsPanel.Init(new UIResultsPanel.Data {
			distance = totalDistance,
			persons = personsDropped,
			coins = coinsEarned,
			distanceBest = distanceBest,
			personBest = personBest,
			onAdCollect = () => {
				AdsController.Instance.ShowAd(success => {
					coinsEarned = success ? coinsEarned * 2 : coinsEarned;
					Restart();
				}, "drive_endless");
			},
			onCollect = Restart
		});
		resultsPanel.Show();
		if (distanceBest || personBest) {
			if (distanceBest) {
				PlayerPrefsManager.UserData.distanceBest = totalDistance;
			}
			if (personBest) {
				PlayerPrefsManager.UserData.personsBest = personsDropped.Count;
			}
			PlayerPrefs.Save();
		}
		AnalyticsSystem.RecordDriveEndlessEndEvent(PlayerPrefsManager.UserData.carSelection, totalDistance, personsDropped.Count, coinsEarned);
	}

	private void ShowMissionResultsPanel() {
		bool distanceBest = totalDistance > PlayerPrefsManager.UserData.distanceBest;
		float health = userCar.GetCurrentHealth();
		int stars = health < 0.25f ? 1 : health < 0.5f ? 2 : 3;
		coinsEarned = selectedMission.coins;
		Time.timeScale = 0f;
		missionResultsPanel.Init(new UIMissionResultsPanel.Data {
			item = selectedMission,
			stars = stars,
			distance = totalDistance,
			distanceBest = distanceBest,
			onAdCollect = () => {
				AdsController.Instance.ShowAd(success => {
					coinsEarned = success ? coinsEarned * 2 : coinsEarned;
					Restart();
				}, "drive_mission");
			},
			onCollect = Restart
		});
		missionResultsPanel.Show();
		if (distanceBest) {
			if (distanceBest) {
				PlayerPrefsManager.UserData.distanceBest = totalDistance;
			}
		}
		PlayerPrefsManager.UserData.completedMissions.Insert(0, new CompletedMission {
			person = currentPerson,
			stars = stars
		});
		PlayerPrefs.Save();
		AnalyticsSystem.RecordDriveMissionEndEvent(PlayerPrefsManager.UserData.carSelection, coinsEarned, stars);
	}

	private void AddCoins(int coins) {
		int prevCoins = PlayerPrefsManager.UserData.coins;
		PlayerPrefsManager.UserData.IncreaseCoins(coins, false);
		garagePanel.PlayCoinsAnim(prevCoins, PlayerPrefsManager.UserData.coins);
	}

	private void InitUI(Action onClose) {
		rideTopPanel = UIController.Instance.GetPanel<UIRideTopPanel>();
		garagePanel = UIController.Instance.GetPanel<UIGaragePanel>();
		pausePanel = UIController.Instance.GetPanel<UIPausePanel>();
		resultsPanel = UIController.Instance.GetPanel<UIResultsPanel>();
		missionResultsPanel = UIController.Instance.GetPanel<UIMissionResultsPanel>();
		takeMissionPanel = UIController.Instance.GetPanel<UITakeMissionPanel>();
		
		garagePanel.gameObject.SetActive(true);
		garagePanel.Init(new UIGaragePanel.Data {
			onCloseButton = () => {
				onClose?.Invoke();
			},
			onLeft = () => {
				selectCarController.UpdateSelection(-1);
				GenerateTakeMissionsItems();
			},
			onRight = () => {
				selectCarController.UpdateSelection(1);
				GenerateTakeMissionsItems();
			},
			onEndless = StartDriving,
			onTakeMission = ShowTakeMissionPanel,
			onBuy = selectCarController.BuyCar
		});
		garagePanel.gameObject.SetActive(false);
		
		rideTopPanel.Init(new UIRideTopPanel.Data {
			onPause = () => {
				Time.timeScale = 0f;
				userCar.SetSoundEnabled(false);
				pausePanel.Show();
			}
		});
		
		pausePanel.Init(new UIPausePanel.Data {
			onSettings = () => UIController.Instance.GetPanel<UISettingsPanel>().Show(),
			onRestart = () => {
				if (coinsEarned > 0) {
					ShowResults();
				} else {
					int distance = Mathf.RoundToInt(userCar.transform.position.z - initUserCarPosAndRot.position.z);
					if (distance > PlayerPrefsManager.UserData.distanceBest) {
						PlayerPrefsManager.UserData.distanceBest = distance;
						PlayerPrefsManager.SaveUserData();
					}
					Restart();
				}
			}, 
			onClose = () => {
				Time.timeScale = 1f;
				userCar.SetSoundEnabled(true);
			}
		});
	}

	private void SetUserCarOnHealthUpdate() {
		userCar.OnHealthUpdate = healthProgress => {
			if (!canControlUserCar) {
				return;
			}
			AudioSystem.Play(onLoseHealthClips[Random.Range(0, onLoseHealthClips.Length)]);
			HapticFeedback.VibrateHaptic(HapticFeedback.Type.Medium);
			rideTopPanel.UpdateHealthSlider(healthProgress);
			if (selectedMission == null && healthProgress < Mathf.Epsilon) {
				StartCoroutine(OnUserCarEnd());	
			}
		};
	}

	private void StartDriving() {
		garagePanel.Close();

		userCar = selectCarController.GetUserCarAndGo();
		initUserCarPosAndRot = new PosAndRot(userCar.transform);
		totalDistance = 0;

		if (selectedMission == null) {
			AnalyticsSystem.RecordDriveEndlessStartEvent(selectCarController.GetCarSelection());	
		} else {
			AnalyticsSystem.RecordDriveMissionStartEvent(selectCarController.GetCarSelection(), selectedMission.coins);
		}
		
		userCar.OnRequireNewSegments = () => {
			trackGenerator.Generate(generatedDirs[0], generatedDirs[1]);
			userCar.SetSegments(trackGenerator.GetCurrentSegment(), trackGenerator.GetNextSegment(generatedDirs[1]), generatedDirs[1]);
			generatedDirs.RemoveAt(0);
			currentGeneratedDirsIndex--;
		};
		userCar.OnPassIntersection = () => {
			Segment currentSegment = trackGenerator.GetCurrentSegment();
			totalDistance += currentSegment.Length + currentSegment.Width;
			
			SetNextGenerateDir();
			
			if (personsEnabled && personPickupController.State == PickupState.Pickup && personPickupSegments == 2) {
				rideTopPanel.Navigation.NextItem(new UINavigationItem.PersonData {
					group = currentPerson.group,
					index = currentPerson.index,
					state = UINavigationItem.PersonData.State.DropOff
				});
			} else {
				rideTopPanel.Navigation.NextItem(new UINavigationItem.DirData { index = (int)GetNextGeneratedDir() });
			}
			
			if (personsEnabled) {
				if (personPickupController.State == PickupState.None) {
					if (selectedMission != null) {
						SetDropOff(trackGenerator.GetNextSegment(generatedDirs[0]));
					} else {
						SetPickUp(trackGenerator.GetNextSegment(generatedDirs[0]));	
					}
				} else if (personPickupController.State == PickupState.Pickup) {
					personPickupSegments--;
					if (personPickupSegments == 0) {
						SetDropOff(trackGenerator.GetNextSegment(generatedDirs[0]));
					}
				}
			}
		};
		SetUserCarOnHealthUpdate();
		
		personsDropped.Clear();
		coinsEarned = 0;
		InitUserCar(() => {
			rideTopPanel.HideDistance();
			rideTopPanel.Show();
		});
		
		trackGenerator.SpawnAICars();
	}

	private void StartMission(UIMissionsList.ItemData missionItem) {
		currentPerson = missionItem.person;
		selectedMission = missionItem;
		personPickupSegmentsList.Insert(0, missionItem.intersections);
		takeMissionPanel.Close();
		StartDriving();
	}

	private void GenerateTakeMissionsItems() {
		missionsList = new List<UIMissionsList.ItemData>();
		
		int count = Random.Range(5, 7);
		List<int> indexes = new();
		for (int i = 0; i < Settings.Instance.personNames.Length; i++) {
			indexes.Add(i);
		}

		int coinsMultiplier = 500 * PlayerPrefsManager.UserData.floors.Length * Mathf.Max(1, selectCarController.GetCarSelection());
		for (int i = 0; i < count; i++) {
			int randomIndex = indexes[Random.Range(0, indexes.Count)];
			int intersections = Random.Range(1, 8);
			int coins = intersections * coinsMultiplier;
			missionsList.Add(new UIMissionsList.ItemData {
				person = new CurrentPerson {
					group = randomIndex <= 8 ? 0 : 1,
					index = randomIndex <= 8 ? randomIndex : randomIndex - 9
				},
				intersections = intersections,
				coins = coins,
				onSelect = StartMission
			});
			indexes.Remove(randomIndex);
		}

		missionsList = missionsList.OrderBy(item => item.coins).ToList();
	}
	
	private void ShowTakeMissionPanel() {
		takeMissionPanel.Show();
		takeMissionPanel.Init(new UITakeMissionPanel.Data {
			itemsData = missionsList
		});
	}

	private void InitUserCar(Action onCanControlCar) {
		userCar.transform.SetPosAndRot(initUserCarPosAndRot);
		userCar.SetAudioVolume(PlayerPrefsManager.UserData.volumes[(int)MixerType.CarEngine]);
		generatedDirs.Add(GenerateDir.Forward);
		Segment nextSegment = trackGenerator.GetNextSegment(generatedDirs[0]);
		userCar.SetSegments(trackGenerator.GetCurrentSegment(), nextSegment, generatedDirs[0]);
		
		userCar.SetStartPoints();
		userCar.GoToStart(camera, () => {
			if (!Input.GetMouseButton(0)) {
				inputManager.ResetValues();	
			}
			canControlUserCar = true;
			drivingInput.gameObject.SetActive(true);
			onCanControlCar();
		});

		currentGeneratedDirsIndex = personsEnabled ? 0 : 1;
		SetNextGenerateDir();
		if (personsEnabled) {
			if (selectedMission == null) {
				SetRandomCurrentPersonIndex();	
			}
			SetPickUp(trackGenerator.GetNextSegment(generatedDirs[0]));
			rideTopPanel.Navigation.Init(new UINavigationItem.DirData { index = (int)generatedDirs[0] },
				new UINavigationItem.PersonData {
					group = currentPerson.group,
					index = currentPerson.index
				});
		} else {
			rideTopPanel.Navigation.Init(new UINavigationItem.DirData { index = (int)generatedDirs[0] },
				new UINavigationItem.DirData { index = (int)generatedDirs[1] });
		}
	}

	private void Restart() {
		Time.timeScale = 0f;
		UIController.Instance.FadeInToBlack(() => {
			if (pausePanel.gameObject.activeSelf) {
				pausePanel.Close(false);	
			}
			rideTopPanel.Close(false);
			if (resultsPanel.gameObject.activeSelf) {
				resultsPanel.Close(false);	
			}
			if (missionResultsPanel.gameObject.activeSelf) {
				missionResultsPanel.Close(false);
			}
			rideTopPanel.ResetItems();
			
			canControlUserCar = false;
			drivingInput.gameObject.SetActive(false);
			userCar.ResetCar();
			userCar.gameObject.SetActive(false);
			userCar.transform.SetPosAndRot(initUserCarPosAndRot);
			userCar = null;
			
			smoke.gameObject.SetActive(false);
			smoke.transform.parent = transform;
			
			generatedDirs.Clear();
			generatedDirs.Add(GenerateDir.Forward);
			trackGenerator.ClearAndReset(GenerateDir.Forward);
					
			camera.transform.SetPosAndRot(initCameraPosAndRot);
			skyline.transform.position = Vector3.zero;
			
			selectCarController.ReInit();

			selectedMission = null;
			GenerateTakeMissionsItems();
			
			garagePanel.Show();
			if (coinsEarned > 0) {
				GameController.Instance.PauseUpdateCoins = true;
				UIController.Instance.ActivateTouchBlocker(2f);
				this.WaitForFrames(1, () => AddCoins(coinsEarned));
				this.Wait(2f, () => {
					GameController.Instance.PauseUpdateCoins = false;
				});
			}
			UIController.Instance.FadeOutFromBlack();
			Time.timeScale = 1f;
		});
	}

	public UserCar GetUserCar() {
		return userCar;
	}

	private GenerateDir GetNextGeneratedDir() {
		return generatedDirs[++currentGeneratedDirsIndex];
	}
	
	private void SetNextGenerateDir() {
		generatedDirs.Add((GenerateDir)Random.Range(0, 3));
	}

	private void SetRandomCurrentPersonIndex() {
		if (Utils.CoinFlip()) {
			currentPerson = new CurrentPerson {
				group = 0,
				index = Random.Range(0, 9)
			};
		} else {
			currentPerson = new CurrentPerson {
				group = 1,
				index = Random.Range(0, 19)
			};
		}
	}

	private void SetPickUp(Segment nextSegment) {
		Vector3 pos = nextSegment.transform.position + nextSegment.transform.forward * nextSegment.Length / 2f +
		              nextSegment.transform.right * (nextSegment.Width - 3.5f); 
		personPickupController.SetPickUp(pos, nextSegment.transform, userCar, currentPerson.group, currentPerson.index);
	}

	private void SetDropOff(Segment nextSegment) {
		Vector3 pos = nextSegment.transform.position + nextSegment.transform.forward * nextSegment.Length / 2f +
		              nextSegment.transform.right * (nextSegment.Width - 3.5f); 
		personPickupController.SetEndPin(pos, nextSegment.transform);
	}
	
	private void Update() {
		if (canControlUserCar) {
			userCar.UpdateCar(inputManager.VerticalInput, inputManager.HorizontalInput);
			skyline.transform.position = userCar.transform.position;
		}
	}
	
	public static Sprite GetPersonSprite(int group, int index) {
		return Resources.Load<Sprite>($"Persons/{group}/Person{index}");
	}
	
	[Serializable]
	public class CurrentPerson {
		public int group;
		public int index;
		
		public Sprite GetSprite() {
			return GetPersonSprite(group, index);
		}
	}
}

[Serializable]
public class CompletedMission {
	public RideController.CurrentPerson person;
	public int stars;
}
