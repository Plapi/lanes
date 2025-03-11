using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour {
	private void Start() {
		AdsController.Instance.Init(() => {
			AdsController.Instance.LoadAd();
			UIController.Instance.FadeInToBlack(() => {
				SceneManager.LoadScene(PlayerPrefsManager.UserData.isTutorialDone ? "Game" : "Tutorial");	
			});
		});
	}
}
