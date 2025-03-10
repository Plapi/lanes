using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour {
	private void Awake() {
		SceneManager.LoadScene(PlayerPrefsManager.UserData.isTutorialDone ? "Game" : "Tutorial");
	}
}
