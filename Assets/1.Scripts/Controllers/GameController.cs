using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviourSingleton<GameController> {

	[SerializeField] private CameraController cameraController;
	[SerializeField] private CompanyController companyController;
	[SerializeField] private RideController rideController;
	
	private UIMainPanel mainPanel;
	private UIRoomPanel roomPanel;
	
	private void Start() {
		Application.targetFrameRate = 60;
		if (Settings.Instance.testMode) {
			gameObject.AddComponent<DebugFPS>();
		}
		
		AudioSystem.Init(this, PlayerPrefsManager.UserData.volumes);
		HapticFeedback.SetEnabled(PlayerPrefsManager.UserData.hapticFeedback);
		
		UIController.Instance.Init();

		roomPanel = UIController.Instance.GetPanel<UIRoomPanel>();
		companyController.Init(room => {
			cameraController.SetEnabled(false);
			cameraController.Zoom(room.GetCameraZoom());
			mainPanel.ShowSettingsButton(false);
			roomPanel.Show();
			roomPanel.Init(new UIRoomPanel.Data {
				roomData = room.RoomData,
				onClose = () => {
					cameraController.SetEnabled(true);
					cameraController.ZoomBack();
					mainPanel.ShowSettingsButton(true);
				},
				onUpgrade = () => {
					UIController.Instance.ActivateTouchBlocker(2f);
					PlayerPrefsManager.UserData.coins -= room.RoomData.UpgradeCost;
					room.RoomData.level++;
					PlayerPrefsManager.SaveUserData();
					mainPanel.CoinsPanel.ConsumeCoins(PlayerPrefsManager.UserData.coins);
					room.UpdateRoomGraphic();
				},
			});
		});
		rideController.Init(OnCompanyButton);
		
		mainPanel = UIController.Instance.GetPanel<UIMainPanel>();
		mainPanel.Init(new UIMainPanel.Data {
			onSettingsButton = () => {
				
			}, onDriversButton = () => {
				
			}, onMultipleCashButton = () => {
				
			}, onDriveButton = OnDriveButton
		});

		TrackGenerator.Instance.OnStartSegmentSetActive += active => {
			companyController.gameObject.SetActive(active);
		};
		
		StartCoroutine(CoinsMechanic());
	}
	
	private void OnDriveButton() {
		UIController.Instance.FadeInToBlack(() => {
			cameraController.gameObject.SetActive(false);
			TrackGenerator.Instance.SetSpawnAICarDistance(90, 110, 20, 40);
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
			mainPanel.Show();
			rideController.Deactivate();
			companyController.Activate();
			UIController.Instance.FadeOutFromBlack();
		});
	}

	private IEnumerator CoinsMechanic() {

		int income;
		while (!TryGetCoinsIncome(out income)) {
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
		
		yield return new WaitUntil(() => mainPanel.gameObject.activeSelf);
		
		mainPanel.CoinsPanel.UpdateProgress(1f);
		PlayerPrefsManager.UserData.IncreaseCoins(income);
		if (roomPanel.gameObject.activeSelf) {
			roomPanel.UpdateUpgradeButton();
		}
		
		mainPanel.CoinsPanel.PlayCoinsIncomeAnim(() => StartCoroutine(CoinsMechanic()));
	}

	private void UpdateCoins(int income) {
		int coins = PlayerPrefsManager.UserData.coins;
		mainPanel.CoinsPanel.UpdateCoins(coins, income);
		companyController.VaultRoom.UpdateTables(coins);
	}

	private static bool TryGetCoinsIncome(out int income) {
		RoomData[] roomData = {
			PlayerPrefsManager.UserData.waitingRoom,
			PlayerPrefsManager.UserData.callCenterRoom
		};
		income = 0;
		for (int i = 0; i < roomData.Length; i++) {
			income += roomData[i].CoinsIncome;
		}
		return income > 0;
	}
}
