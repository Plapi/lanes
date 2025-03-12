using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour {
	private void Start() {
		if (Settings.Instance.enableAnalytics) {
			UnityServices.InitializeAsync();
			AnalyticsService.Instance.StartDataCollection();	
		}
		if (Settings.Instance.enableAdds) {
			AdsController.Instance.Init(() => {
				AdsController.Instance.LoadAd();
			});	
		}
		SceneManager.LoadSceneAsync(PlayerPrefsManager.UserData.isTutorialDone ? "Game" : "Tutorial");
	}
}
