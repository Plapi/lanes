using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour {
	private void Start() {
		UnityServices.InitializeAsync();
		AnalyticsService.Instance.StartDataCollection();
#if !UNITY_EDITOR
		AdsController.Instance.Init(() => {
			AdsController.Instance.LoadAd();
		});
#endif
		SceneManager.LoadSceneAsync(PlayerPrefsManager.UserData.isTutorialDone ? "Game" : "Tutorial");
	}
}
