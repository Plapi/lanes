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
					
				},
			});
		});
		rideController.Init(OnCompanyButton);
		
		mainPanel = UIController.Instance.GetPanel<UIMainPanel>();
		mainPanel.Init(new UIMainPanel.Data {
			coins = PlayerPrefsManager.UserData.coins,
			income = 100,
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
		const float time = 10f;
		float rTime = 0f;
		while (rTime < time) {
			yield return null;
			rTime += Time.deltaTime;
			mainPanel.CoinsPanel.UpdateProgress(rTime / time);
		}
		mainPanel.CoinsPanel.UpdateProgress(1f);
		yield return new WaitUntil(() => mainPanel.gameObject.activeSelf);
		PlayerPrefsManager.UserData.coins += 100;
		mainPanel.CoinsPanel.PlayCoinsIncomeAnim(() => {
			mainPanel.CoinsPanel.UpdateCoins(PlayerPrefsManager.UserData.coins, 100);
			StartCoroutine(CoinsMechanic());
		});
	}
}
