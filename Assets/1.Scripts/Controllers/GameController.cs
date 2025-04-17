using UnityEngine;

public class GameController : MonoBehaviourSingleton<GameController> {

	[SerializeField] private CameraController cameraController;
	[SerializeField] private CompanyController companyController;
	[SerializeField] private RideController rideController;
	
	private UIMainPanel mainPanel;
	
	private void Start() {
		AudioSystem.Init(this, PlayerPrefsManager.UserData.volumes);
		HapticFeedback.SetEnabled(PlayerPrefsManager.UserData.hapticFeedback);
		
		UIController.Instance.Init();
		
		companyController.Init();
		rideController.Init(OnCompanyButton);
		
		mainPanel = UIController.Instance.GetPanel<UIMainPanel>();
		mainPanel.Init(new UIMainPanel.Data {
			onDriversButton = () => {
				
			}, onMultipleCashButton = () => {
				
			}, ondriveButton = OnDriveButton
		});

		TrackGenerator.Instance.OnStartSegmentSetActive += active => {
			companyController.gameObject.SetActive(active);
		};
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
}
