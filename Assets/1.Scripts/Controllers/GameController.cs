using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviourSingleton<GameController> {

	[SerializeField] private Camera mainCamera;
	[SerializeField] private InputManager inputManager;
	[SerializeField] private Transform skyline;
	[SerializeField] private StartSegment startSegment;
	[SerializeField] private SelectCarController selectCarController;
	[SerializeField] private GameObject smoke;
	[SerializeField] private PersonPickupController personPickupController;
	
	[Space]
	[SerializeField] private AudioClip[] onLoseHealthClips;
	
	[Space]
	[SerializeField] private bool aiCarsEnabled;
	
	private UserCar userCar;
	
	private readonly List<Segment> segments = new(4);
	private Segment currentSegment;
	private Segment leftSegment;
	private Segment rightSegment;
	private Segment nextSegment;
	private Intersection intersection;
	
	private AICar[] aiCarPrefabs;
	private bool canControlUserCar;
	
	private UITopPanel topPanel;
	private UIGaragePanel garagePanel;
	private UIPausePanel pausePanel;
	private UIResultsPanel resultsPanel;
	private UISettingsPanel settingsPanel;
	
	private PosAndRot initCameraPosAndRot;
	private PosAndRot initUserCarPosAndRot;

	private int personPickupSegments;
	private int personsDropped;
	private int coinsEarned;
	
	protected void Start() {
		AudioSystem.Init(this, PlayerPrefsManager.UserData.volumes);
		HapticFeedback.SetEnabled(PlayerPrefsManager.UserData.hapticFeedback);
		initCameraPosAndRot = new PosAndRot(mainCamera.transform);
		InitFirstSegments();
		InitPersonPickupController();
		InitUI();
		selectCarController.Init();
	}

	private void InitPersonPickupController() {
		float startZ = 0f;
		List<int> personPickupSegmentsList = new() { 1, Random.Range(1, 3), Random.Range(2, 4) };
		personPickupController.OnPickup = () => {
			if (personPickupSegmentsList.Count > 0) {
				personPickupSegments = personPickupSegmentsList[0];
				personPickupSegmentsList.RemoveAt(0);
			} else {
				personPickupSegments = Random.Range(1, 6);	
			}
			topPanel.ShowPerson(personPickupSegments);
			startZ = userCar.transform.position.z;
		};
		personPickupController.OnDrop = () => {
			personsDropped++;
			float distance = userCar.transform.position.z - startZ;
			int coins = Mathf.RoundToInt(userCar.CoinsMultiplier * Mathf.Lerp(50, 500, Mathf.InverseLerp(100f, 2500f, distance)));
			topPanel.HidePerson(coins);
			coinsEarned += coins;
		};
		personPickupController.OnMiss = () => {
			topPanel.HidePerson(-1);
		};
		personPickupController.OnUpdateDistance = distance => {
			topPanel.ShowDistance(distance);
		};
	}

	private IEnumerator OnUserCarEnd() {
		smoke.transform.parent = userCar.transform;
		smoke.transform.position = userCar.FrontPos;
		smoke.gameObject.SetActive(true);
		userCar.UpdateCar(0f, 0.5f);
		canControlUserCar = false;
		
		float time = 4f;
		while (time > 0f && !Input.GetMouseButtonDown(0)) {
			yield return null;
			time -= Time.deltaTime;
		}
		
		ShowResults();
	}

	private void ShowResults() {
		int distance = Mathf.RoundToInt(userCar.transform.position.z - initUserCarPosAndRot.position.z);
		bool distanceBest = distance > PlayerPrefsManager.UserData.distanceBest;
		bool personBest = personsDropped > PlayerPrefsManager.UserData.personsBest;
		Time.timeScale = 0f;
		resultsPanel.Init(new UIResultsPanel.Data {
			distance = distance,
			persons = personsDropped,
			coins = coinsEarned,
			distanceBest = distanceBest,
			personBest = personBest,
			onAdCollect = () => {
				AdsController.Instance.ShowAd(success => {
					coinsEarned = success ? coinsEarned * 2 : coinsEarned;
					Restart();
				});
			},
			onCollect = Restart
		});
		resultsPanel.Show();
		if (distanceBest || personBest) {
			if (distanceBest) {
				PlayerPrefsManager.UserData.distanceBest = distance;
			}
			if (personBest) {
				PlayerPrefsManager.UserData.personsBest = personsDropped;
			}
			PlayerPrefs.Save();
		}
		AnalyticsSystem.RecordRaceEndEvent(PlayerPrefsManager.UserData.carSelection, distance, personsDropped, coinsEarned);
	}

	private void AddCoins(int coins) {
		garagePanel.PlayCoinsAnim(PlayerPrefsManager.UserData.coins, PlayerPrefsManager.UserData.coins + coinsEarned);
		PlayerPrefsManager.UserData.coins += coins;
		PlayerPrefsManager.SaveUserData();
	}

	private void InitUI() {
		UIController.Instance.Init();
		
		topPanel = UIController.Instance.GetPanel<UITopPanel>();
		garagePanel = UIController.Instance.GetPanel<UIGaragePanel>();
		pausePanel = UIController.Instance.GetPanel<UIPausePanel>();
		resultsPanel = UIController.Instance.GetPanel<UIResultsPanel>();
		settingsPanel = UIController.Instance.GetPanel<UISettingsPanel>();
		
		garagePanel.Init(new UIGaragePanel.Data {
			onSettings = settingsPanel.Show,
			onLeft = () => selectCarController.UpdateSelection(-1),
			onRight = () => selectCarController.UpdateSelection(1),
			onGo = Go,
			onBuy = selectCarController.BuyCar,
			onCoin = () => {
				PlayerPrefsManager.UserData.coins += 10000;
				PlayerPrefsManager.SaveUserData();
				garagePanel.UpdateCoins(PlayerPrefsManager.UserData.coins);
			},
			coins = PlayerPrefsManager.UserData.coins
		});
		
		topPanel.Init(new UITopPanel.Data {
			onPause = () => {
				Time.timeScale = 0f;
				userCar.SetSoundEnabled(false);
				pausePanel.Show();
			}
		});
		
		pausePanel.Init(new UIPausePanel.Data {
			onSettings = settingsPanel.Show,
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

		settingsPanel.Init(new UISettingsPanel.Data {
			volumes = PlayerPrefsManager.UserData.volumes,
			onUpdateSlider = (index, volume) => {
				MixerType mixerType = (MixerType)index;
				if (mixerType == MixerType.CarEngine) {
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
						string subject = EscapeURL("Feedback about Quick Lane Driver");
						string body = EscapeURL("Hi, I’d like to share my thoughts about the game...");
						string mailto = $"mailto:{email}?subject={subject}&body={body}";
						Application.OpenURL(mailto);
					}
				}).Show();
			}, onTutorial = () => {
				UIController.Instance.FadeInToBlack(() => {
					SceneManager.LoadScene("Tutorial");
				});
			}
		});
	}
	
	private static string EscapeURL(string text) {
		return UnityEngine.Networking.UnityWebRequest.EscapeURL(text).Replace("+", "%20");
	}

	private void Go() {
		garagePanel.Close();

		userCar = selectCarController.GetUserCarAndGo();
		initUserCarPosAndRot = new PosAndRot(userCar.transform);
		
		userCar.OnRequireNewSegments = () => {
			if (userCar.CurrentSegment != startSegment) {
				startSegment.ClearAICars();
				InitNextSegments();
			}
			userCar.SetSegments(currentSegment, nextSegment);
		};
		userCar.OnHealthUpdate = healthProgress => {
			if (!canControlUserCar) {
				return;
			}
			AudioSystem.Play(onLoseHealthClips[Random.Range(0, onLoseHealthClips.Length)]);
			HapticFeedback.VibrateHaptic(HapticFeedback.Type.Medium);
			topPanel.UpdateHealthSlider(healthProgress);
			if (healthProgress < Mathf.Epsilon) {
				StartCoroutine(OnUserCarEnd());	
			}
		};
		
		personsDropped = 0;
		coinsEarned = 0;
		InitUserCar(() => {
			topPanel.HideDistance();
			topPanel.Show();
			SetPickUp();
		});
		
		if (aiCarsEnabled) {
			currentSegment.SpawnAICars();
			SpawnAICars();	
		}
	}

	private void InitUserCar(Action onCanControlCar) {
		userCar.transform.SetPosAndRot(initUserCarPosAndRot);
		userCar.SetAudioVolume(PlayerPrefsManager.UserData.volumes[(int)MixerType.CarEngine]);
		userCar.SetSegments(startSegment, currentSegment);
		userCar.SetStartPoints();
		userCar.GoToStart(mainCamera, () => {
			if (!Input.GetMouseButton(0)) {
				inputManager.ResetValues();	
			}
			canControlUserCar = true;
			onCanControlCar();
		});
	}

	private void SetPickUp() {
		Transform lane = nextSegment.Lanes[^1].transform;
		float z = lane.position.z + nextSegment.Length / 4f;
		personPickupController.SetPickUp(new Vector3(lane.position.x + 1.5f, 0f, z), userCar);
	}

	private void Restart() {
		Time.timeScale = 0f;
		UIController.Instance.FadeInToBlack(() => {

			if (pausePanel.gameObject.activeSelf) {
				pausePanel.Close(false);	
			}
			topPanel.Close(false);
			if (resultsPanel.gameObject.activeSelf) {
				resultsPanel.Close(false);	
			}
			topPanel.ResetItems();
			
			canControlUserCar = false;
			userCar.ResetCar();
			userCar.gameObject.SetActive(false);
			userCar.transform.SetPosAndRot(initUserCarPosAndRot);
			userCar = null;
			
			smoke.gameObject.SetActive(false);
			smoke.transform.parent = transform;

			for (int i = 0; i < segments.Count; i++) {
				segments[i].Clear();
			}
			segments.Clear();
			intersection.Clear();
			startSegment.ClearAICars();
			InitFirstSegments(true);
					
			mainCamera.transform.SetPosAndRot(initCameraPosAndRot);
			skyline.transform.position = Vector3.zero;
			
			selectCarController.ReInit();
					
			garagePanel.Show();
			if (coinsEarned > 0) {
				AddCoins(coinsEarned);	
			}
			UIController.Instance.FadeOutToBlack();
			Time.timeScale = 1f;

			
		});
	}

	public UserCar GetUserCar() {
		return userCar;
	}
	
	private void Update() {
		if (canControlUserCar) {
			userCar.UpdateCar(inputManager.VerticalInput, inputManager.HorizontalInput);
			skyline.transform.position = userCar.transform.position;
		}
	}

	private void InitFirstSegments(bool restart = false) {
		startSegment.Init(GetSegmentData(new SegmentInputData {
			backLanes = 2,
			frontLanes = 2,
			length = 200
		}));
		if (!restart) {
			startSegment.SetStartAndEndPosForRoadLanes();	
		}
		currentSegment = NewSegment("CurrentSegment", GetSegmentData(new SegmentInputData {
			backLanes = 2,
			frontLanes = 2,
			length = 50
		}));
		segments.Add(currentSegment);
		CreateNextSegments();
		for (int i = 0; i < segments.Count; i++) {
			segments[i].SetStartAndEndPosForRoadLanes();
		}
		intersection.CreateRoadConnections();
		ConnectCurrentSegmentWithStartSegment();
		if (!restart) {
			startSegment.CreateBottomLeftEnvironment(leftSegment);
			startSegment.CreateRightEnvironment(rightSegment);	
		}
		nextSegment.CreteTopLeftEnvironment(leftSegment);
		nextSegment.CreteTopRightEnvironment(rightSegment);
	}

	private void InitNextSegments() {
		segments.Clear();
		currentSegment.Clear();
		leftSegment.Clear();
		rightSegment.Clear();
		intersection.Clear();
		currentSegment = nextSegment;
		currentSegment.name = "CurrentSegment";
		currentSegment.ClearNextRoadLanes();
		segments.Add(currentSegment);
		
		CreateNextSegments();
		for (int i = 0; i < segments.Count; i++) {
			segments[i].SetStartAndEndPosForRoadLanes();
		}
		intersection.CreateRoadConnections();
		
		currentSegment.ContinueGenerateEnvIfNeeded(leftSegment, rightSegment);
		nextSegment.CreteTopLeftEnvironment(leftSegment);
		nextSegment.CreteTopRightEnvironment(rightSegment);

		if (aiCarsEnabled) {
			SpawnAICars();
		}

		if (personPickupController.State == PickupState.None) {
			SetPickUp();
		} else if (personPickupController.State == PickupState.Pickup) {
			personPickupSegments--;
			if (personPickupSegments == 0) {
				Transform lane = nextSegment.Lanes[^1].transform;
				float z = lane.position.z + nextSegment.Length / 4f;
				personPickupController.SetEndPin(new Vector3(lane.position.x + 1.5f, 0f, z));
			}
		} 
	}

	private void SpawnAICars() {
		leftSegment.SpawnAICars(false);
		rightSegment.SpawnAICars(true, false);
		nextSegment.SpawnAICars();
	}
	
	private void CreateNextSegments() {
		leftSegment = NewSegment("LeftSegment", GetRandomSegmentData(),-90f);
		rightSegment = NewSegment("RightSegment", GetRandomSegmentData(), -90f);
		nextSegment = NewSegment("NextSegment", GetRandomSegmentData());
		
		segments.Add(leftSegment);
		segments.Add(rightSegment);
		segments.Add(nextSegment);
		
		leftSegment.AlignHorizontalWith(rightSegment);
		nextSegment.AlignVerticalWith(currentSegment);
		
		leftSegment.transform.SetLocalX(Mathf.Min(currentSegment.transform.localPosition.x, nextSegment.transform.localPosition.x));
		rightSegment.transform.SetLocalX(Mathf.Max(currentSegment.transform.localPosition.x + currentSegment.Width, nextSegment.transform.localPosition.x + nextSegment.Width) + rightSegment.Length);
		
		float addZ = currentSegment.transform.localPosition.z + currentSegment.Length - Mathf.Min(leftSegment.transform.localPosition.z, rightSegment.transform.localPosition.z);
		leftSegment.transform.SetLocalZ(leftSegment.transform.localPosition.z + addZ);
		rightSegment.transform.SetLocalZ(rightSegment.transform.localPosition.z + addZ);
		
		nextSegment.transform.SetLocalZ(Mathf.Max(leftSegment.transform.localPosition.z + leftSegment.Width, rightSegment.transform.localPosition.z + rightSegment.Width));
		
		intersection = Instantiate(Resources.Load<Intersection>("Intersection/Intersection"), transform);
		intersection.name = "Intersection";
		intersection.transform.SetLocalX(leftSegment.transform.localPosition.x);
		intersection.transform.SetLocalZ(currentSegment.transform.localPosition.z + currentSegment.Length);
		intersection.Init(currentSegment, leftSegment, rightSegment, nextSegment);
	}

	private Segment NewSegment(string segmentName, SegmentData segmentData, float angle = 0f) {
		Segment segment = new GameObject(segmentName).AddComponent<Segment>();
		segment.transform.parent = transform;
		segment.Init(segmentData);
		segment.transform.SetLocalAngleY(angle);
		return segment;
	}

	private void ConnectCurrentSegmentWithStartSegment() {
		for (int i = 0; i < currentSegment.BackRoadLanes.Count; i++) {
			RoadLane lane0 = currentSegment.BackRoadLanes[i];
			RoadLane lane1 = startSegment.BackRoadLanes[i];
			lane0.AddNextRoadLane(lane1, new List<Vector3> { lane0.EndPos, lane1.StartPos });
		}
	}

	private static SegmentData GetRandomSegmentData() {
		SegmentInputData segmentInputData = new() {
			backLanes = Random.Range(1, 5),
			frontLanes = Random.Range(1, 5),
			length = Settings.Instance.laneSize * Random.Range(40, 100)
		};
		return GetSegmentData(segmentInputData);
	}

	public static SegmentData GetSegmentData(SegmentInputData segmentInputData) {
		List<LaneData> lanes = new() {
			new LaneData {
				type = LaneType.SideWalkLaneLeft
			}
		};
		
		int backLanes = segmentInputData.backLanes;
		int frontLanes = segmentInputData.frontLanes;

		if (backLanes > 1) {
			lanes.Add(new RoadLaneData {
				type = LaneType.RoadLaneSingleLeft
			});	
			for (int i = 1; i < backLanes - 1; i++) {
				lanes.Add(new RoadLaneData {
					type = LaneType.RoadLaneMiddle
				});	
			}
		}
		lanes.Add(new RoadLaneData {
			type = LaneType.RoadLaneEdgeLeft
		});
		
		lanes.Add(new RoadLaneData {
			type = LaneType.RoadLaneEdgeRight,
			hasFrontDirection = true,
		});
		if (frontLanes > 1) {
			for (int i = 1; i < frontLanes - 1; i++) {
				lanes.Add(new RoadLaneData {
					type = LaneType.RoadLaneMiddle,
					hasFrontDirection = true
				});	
			}
			lanes.Add(new RoadLaneData {
				type = LaneType.RoadLaneSingleRight,
				hasFrontDirection = true,
			});
		}
		
		lanes.Add(new LaneData {
			type = LaneType.SideWalkLaneRight
		});
		for (int i = 0; i < lanes.Count; i++) {
			lanes[i].length = segmentInputData.length;
		}
		
		return new SegmentData {
			lanes = lanes.ToArray()
		};
	}
	
}

public class SegmentInputData {
	[Range(1, 4)] public int backLanes = 2;
	[Range(1, 4)] public int frontLanes = 2;
	public int length;
}
