using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
	private UISettingsPanel settingsPanel;
	
	private PosAndRot initCameraPosAndRot;
	private PosAndRot initUserCarPosAndRot;

	private int totalDistance;
	private int personPickupSegments;
	private readonly List<int> personsDropped = new();
	private int coinsEarned;

	private readonly List<GenerateDir> generatedDirs = new();
	private int currentGeneratedDirsIndex;

	private int currentPersonIndex = -1;

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
	}

	public void Deactivate() {
		camera.gameObject.SetActive(false);
		trackGenerator.ClearAllSegments();
		rotateObjController.enabled = false;
	}

	private void InitPersonPickupController() {
		int startDistance = 0;
		List<int> personPickupSegmentsList = new() { 1, Random.Range(1, 3), Random.Range(2, 4) };
		personPickupController.OnPickup = () => {
			if (personPickupSegmentsList.Count > 0) {
				personPickupSegments = personPickupSegmentsList[0];
				personPickupSegmentsList.RemoveAt(0);
			} else {
				personPickupSegments = Random.Range(1, 6);	
			}
			rideTopPanel.ShowPerson(personPickupSegments, Settings.Instance.personSprites[currentPersonIndex]);
			startDistance = totalDistance;
			
			rideTopPanel.Navigation.UpdateCurrentPersonState(UINavigationItem.PersonData.State.Checkmark);
			if (personPickupSegments > 1) {
				rideTopPanel.Navigation.NextItem(new UINavigationItem.DirData { index = (int)GetNextGeneratedDir() });
			} else {
				rideTopPanel.Navigation.NextItem(new UINavigationItem.PersonData {
					index = currentPersonIndex,
					state = UINavigationItem.PersonData.State.DropOff
				});
			}
		};
		personPickupController.OnNotPickup = () => {
			rideTopPanel.Navigation.UpdateCurrentPersonState(UINavigationItem.PersonData.State.Missed);
			GenerateNewPerson();
		};
		personPickupController.OnDrop = () => {
			personsDropped.Add(currentPersonIndex);
			float distance = totalDistance - startDistance + Vector3.Distance(trackGenerator.GetNextSegment(generatedDirs[0]).transform.position, userCar.transform.position);
			int coins = Mathf.RoundToInt(userCar.CoinsMultiplier * Mathf.Lerp(50, 500, Mathf.InverseLerp(100f, 2500f, distance)));
			rideTopPanel.HidePerson(coins);
			coinsEarned += coins;
			rideTopPanel.Navigation.UpdateCurrentPersonState(UINavigationItem.PersonData.State.Checkmark);
			GenerateNewPerson();
		};
		personPickupController.OnDropMissed = () => {
			rideTopPanel.HidePerson(-1);
			rideTopPanel.Navigation.UpdateCurrentPersonState(UINavigationItem.PersonData.State.Missed);
			GenerateNewPerson();
		};
		personPickupController.OnUpdateDistance = distance => {
			rideTopPanel.ShowDistance(distance);
		};
	}

	private void GenerateNewPerson() {
		SetRandomCurrentPersonIndex();
		rideTopPanel.Navigation.NextItem(new UINavigationItem.PersonData { index = currentPersonIndex });
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
				});
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
		AnalyticsSystem.RecordRaceEndEvent(PlayerPrefsManager.UserData.carSelection, totalDistance, personsDropped.Count, coinsEarned);
	}

	private void AddCoins(int coins) {
		garagePanel.PlayCoinsAnim(PlayerPrefsManager.UserData.coins, PlayerPrefsManager.UserData.coins + coinsEarned);
		PlayerPrefsManager.UserData.coins += coins;
		PlayerPrefsManager.SaveUserData();
	}

	private void InitUI(Action onClose) {
		rideTopPanel = UIController.Instance.GetPanel<UIRideTopPanel>();
		garagePanel = UIController.Instance.GetPanel<UIGaragePanel>();
		pausePanel = UIController.Instance.GetPanel<UIPausePanel>();
		resultsPanel = UIController.Instance.GetPanel<UIResultsPanel>();
		settingsPanel = UIController.Instance.GetPanel<UISettingsPanel>();
		
		garagePanel.gameObject.SetActive(true);
		garagePanel.Init(new UIGaragePanel.Data {
			onCloseButton = () => {
				onClose?.Invoke();
			},
			onSettings = settingsPanel.Show,
			onLeft = () => selectCarController.UpdateSelection(-1),
			onRight = () => selectCarController.UpdateSelection(1),
			onGo = Go,
			onBuy = selectCarController.BuyCar,
			onCoin = () => {
				if (Settings.Instance.testMode) {
					PlayerPrefsManager.UserData.coins += 10000;
					PlayerPrefsManager.SaveUserData();
					garagePanel.UpdateCoins(PlayerPrefsManager.UserData.coins);	
				}
			},
			coins = PlayerPrefsManager.UserData.coins
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
		totalDistance = 0;
		
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
					index = currentPersonIndex,
					state = UINavigationItem.PersonData.State.DropOff
				});
			} else {
				rideTopPanel.Navigation.NextItem(new UINavigationItem.DirData { index = (int)GetNextGeneratedDir() });
			}
			
			if (personsEnabled) {
				if (personPickupController.State == PickupState.None) {
					SetPickUp(trackGenerator.GetNextSegment(generatedDirs[0]));
				} else if (personPickupController.State == PickupState.Pickup) {
					personPickupSegments--;
					if (personPickupSegments == 0) {
						SetDropOff(trackGenerator.GetNextSegment(generatedDirs[0]));
					}
				}
			}
		};
		userCar.OnHealthUpdate = healthProgress => {
			if (!canControlUserCar) {
				return;
			}
			AudioSystem.Play(onLoseHealthClips[Random.Range(0, onLoseHealthClips.Length)]);
			HapticFeedback.VibrateHaptic(HapticFeedback.Type.Medium);
			rideTopPanel.UpdateHealthSlider(healthProgress);
			if (healthProgress < Mathf.Epsilon) {
				StartCoroutine(OnUserCarEnd());	
			}
		};
		
		personsDropped.Clear();
		coinsEarned = 0;
		InitUserCar(() => {
			rideTopPanel.HideDistance();
			rideTopPanel.Show();
		});
		
		trackGenerator.SpawnAICars();
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
			onCanControlCar();
		});

		currentGeneratedDirsIndex = personsEnabled ? 0 : 1;
		SetNextGenerateDir();
		if (personsEnabled) {
			SetRandomCurrentPersonIndex();
			SetPickUp(trackGenerator.GetNextSegment(generatedDirs[0]));
			rideTopPanel.Navigation.Init(new UINavigationItem.DirData { index = (int)generatedDirs[0] },
				new UINavigationItem.PersonData { index = currentPersonIndex });
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
			rideTopPanel.ResetItems();
			
			canControlUserCar = false;
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
					
			garagePanel.Show();
			if (coinsEarned > 0) {
				AddCoins(coinsEarned);	
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
		List<Sprite> list = new(Settings.Instance.personSprites);
		if (currentPersonIndex != -1) {
			list.RemoveAt(currentPersonIndex);
		}
		currentPersonIndex = Random.Range(0, list.Count);
	}

	private void SetPickUp(Segment nextSegment) {
		Vector3 pos = nextSegment.transform.position + nextSegment.transform.forward * nextSegment.Length / 2f +
		              nextSegment.transform.right * (nextSegment.Width - 3.5f); 
		personPickupController.SetPickUp(pos, nextSegment.transform, userCar, currentPersonIndex);
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
}
