using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviourSingleton<GameController> {

	[SerializeField] private Camera mainCamera;
	[SerializeField] private InputManager inputManager;
	[SerializeField] private Transform skyline;
	[SerializeField] private StartSegment startSegment;
	[SerializeField] private GameObject smoke;
	[SerializeField] private PersonPickupController personPickupController;
	
	[Space]
	[SerializeField] private bool aiCarsEnabled;
	
	[Space]
	[SerializeField] private UserCar[] userCars;
	
	private UserCar userCar;
	
	private readonly List<Segment> segments = new(4);
	private Segment currentSegment;
	private Segment leftSegment;
	private Segment rightSegment;
	private Segment nextSegment;
	private Intersection intersection;
	
	private AICar[] aiCarPrefabs;
	private bool canControlUserCar;
	private int currentCarSelection;
	
	private UITopPanel topPanel;
	private UIGaragePanel garagePanel;
	private UIPausePanel pausePanel;
	private UIResultsPanel resultsPanel;
	
	private PosAndRot initCameraPosAndRot;
	private PosAndRot initUserCarPosAndRot;

	private int personPickupSegments;
	private int personsDropped;
	private int coinsEarned;

	protected override void Awake() {
		base.Awake();
		InitUserCars();
		initCameraPosAndRot = new PosAndRot(mainCamera.transform);
	}

	protected void Start() {
		InitFirstSegments();
		InitUI();
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
			int coins = (int)Mathf.Lerp(50, 500, Mathf.InverseLerp(100f, 2500f, distance));
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

	private void InitUserCars() {
		for (int i = 0; i < userCars.Length; i++) {
			userCars[i].DisableCar();
			userCars[i].gameObject.SetActive(false);
			userCars[i].OnRequireNewSegments = () => {
				if (userCar.CurrentSegment != startSegment) {
					startSegment.ClearAICars();
					InitNextSegments();
				}
				userCar.SetSegments(currentSegment, nextSegment);
			};
			userCars[i].OnHealthUpdate = healthProgress => {
				if (!canControlUserCar) {
					return;
				}
				topPanel.UpdateHealthSlider(healthProgress);
				if (healthProgress < Mathf.Epsilon) {
					StartCoroutine(OnUserCarEnd());	
				}
			};
		}
		currentCarSelection = Mathf.Clamp(PlayerPrefsManager.UserData.carSelection, 0, userCars.Length - 1);
		userCar = userCars[currentCarSelection];
		userCar.gameObject.SetActive(true);
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
			onAdCollect = Restart,
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
	}

	private void InitUI() {
		UIController.Instance.Init();
		
		topPanel = UIController.Instance.GetPanel<UITopPanel>();
		garagePanel = UIController.Instance.GetPanel<UIGaragePanel>();
		pausePanel = UIController.Instance.GetPanel<UIPausePanel>();
		resultsPanel = UIController.Instance.GetPanel<UIResultsPanel>();
		
		garagePanel.Init(new UIGaragePanel.Data {
			onLeft = () => UpdateUserCarSelection(currentCarSelection - 1),
			onRight = () => UpdateUserCarSelection(currentCarSelection + 1),
			onGo = Go
		});
		garagePanel.SetLeftRightButtonInteractable(currentCarSelection > 0, currentCarSelection < userCars.Length - 1);
		
		topPanel.Init(new UITopPanel.Data {
			onPause = () => {
				Time.timeScale = 0f;
				pausePanel.Show();
			}
		});
		
		pausePanel.Init(new UIPausePanel.Data {
			onSettings = () => {
				
			}, onRestart = Restart, 
			onClose = () => {
				Time.timeScale = 1f;
			}
		});
	}

	private void Go() {
		garagePanel.Close();
			
		PlayerPrefsManager.UserData.carSelection = currentCarSelection;
		PlayerPrefsManager.SaveUserData();
				
		initUserCarPosAndRot = new PosAndRot(userCar.transform);
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

	private void SetPickUp() {
		Transform lane = nextSegment.Lanes[^1].transform;
		float z = lane.position.z + nextSegment.Length / 4f;
		personPickupController.SetPickUp(new Vector3(lane.position.x + 1.5f, 0f, z), userCar);
	}

	private void Restart() {
		Time.timeScale = 0f;
		UIController.Instance.FadeInToBlack(() => {
			
			pausePanel.Close(false);
			topPanel.Close(false);
			resultsPanel.Close(false);
			topPanel.ResetHealthSlider();
					
			canControlUserCar = false;
			userCar.ResetCar();
			userCar.transform.SetPosAndRot(initUserCarPosAndRot);
			
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
					
			garagePanel.Show();
			UIController.Instance.FadeOutToBlack();
			Time.timeScale = 1f;
		});
	}

	private void UpdateUserCarSelection(int selection) {
		selection = Mathf.Clamp(selection, 0, userCars.Length - 1);
		userCar.gameObject.SetActive(false);
		userCar = userCars[selection];
		userCar.gameObject.SetActive(true);
		garagePanel.SetLeftRightButtonInteractable(selection > 0, selection < userCars.Length - 1);
		currentCarSelection = selection;
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

	private void InitUserCar(Action onCanControlCar) {
		userCar.SetSegments(startSegment, currentSegment);
		userCar.SetStartPoints();
		userCar.GoToStart(() => {
			canControlUserCar = true;
			onCanControlCar();
		});
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
			backLanes = Random.Range(1, 4),
			frontLanes = Random.Range(1, 4),
			length = Settings.Instance.laneSize * Random.Range(40, 100)
		};
		return GetSegmentData(segmentInputData);
	}

	private static SegmentData GetSegmentData(SegmentInputData segmentInputData) {
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
	
	[Serializable]
	private class SegmentInputData {
		[Range(1, 4)] public int backLanes = 2;
		[Range(1, 4)] public int frontLanes = 2;
		public int length;
	}
}
