using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour {

	[SerializeField] private Camera mainCamera;
	[SerializeField] private InputManager inputManager;
	[SerializeField] private TrackGenerator trackGenerator;
	[SerializeField] private UserCar userCar;
	[SerializeField] private AudioClip successClip;

	private UITutorialPanel tutorialPanel;

	private readonly string[] tutorialTexts = {
		"Hi, welcome to <color=#FFFF00>Quick Lane Driver!</color>\nBefore you hit the road, let's go through a <color=#FFFF00>quick tutorial</color> to learn how to drive.",
		"To drive your car, <color=#FFFF00>touch and hold</color> the bottom part of the screen. Move your finger <color=#FFFF00>UP</color> to accelerate and <color=#FFFF00>DOWN</color> to slow down. Try it now!",
		"Great! Now let’s learn how to <color=#FFFF00>steer</color>. Move your finger <color=#FFFF00>LEFT</color> to turn left and <color=#FFFF00>RIGHT</color> to turn right. Try it out!",
		"You’ve <color=#FFFF00>mastered</color> the basics! Now hit the road, pick up passengers, and become the <color=#FFFF00>best driver</color> in town!"
	};
	
	private void Start() {
		DontDestroyOnLoad(ObjectPoolManager.Instance);
		AudioSystem.Init(this, PlayerPrefsManager.UserData.volumes);
		
		inputManager.UpdateVerticalInput(0.4f);
		inputManager.enabled = false;
		
		trackGenerator.Init(GenerateDir.Forward);
		
		userCar.ApplyMaterial(PlayerPrefsManager.UserData.carColors[4]);
		userCar.SetAudioVolume(PlayerPrefsManager.UserData.volumes[(int)MixerType.CarEngine]);
		userCar.SetSegments(trackGenerator.GetCurrentSegment(), trackGenerator.GetNextSegment(GenerateDir.Forward), GenerateDir.Forward);
		userCar.SetStartPoints();
		userCar.SetSoundEnabled(true);
		userCar.GetComponent<AudioSource>().enabled = false;
		userCar.SetEngineSoundToCamera(mainCamera);
		userCar.OnRequireNewSegments = () => {
			trackGenerator.Generate(GenerateDir.Forward, GenerateDir.Forward);
			userCar.SetSegments(trackGenerator.GetCurrentSegment(), trackGenerator.GetNextSegment(GenerateDir.Forward), GenerateDir.Forward);
		};

		UIController.Instance.Init();
		tutorialPanel = UIController.Instance.GetPanel<UITutorialPanel>();
		tutorialPanel.Init(new UITutorialPanel.Data());

		StartCoroutine(Tutorial());
	}

	private IEnumerator Tutorial() {

		AnalyticsSystem.RecordTutorialEvent(0);
		
		yield return new WaitForSeconds(2f);
		
		tutorialPanel.ShowText(tutorialTexts[0]);
		userCar.GetComponent<AudioSource>().enabled = true;
		
		yield return new WaitForSeconds(3f);

		bool advance = false;
		tutorialPanel.ShowNextButton(() => {
			advance = true;
		});

		yield return new WaitUntil(() => advance);
		AnalyticsSystem.RecordTutorialEvent(1);
		
		tutorialPanel.HideNextButton();
		tutorialPanel.ShowText(tutorialTexts[1]);
		
		yield return new WaitForSeconds(2f);
		
		inputManager.enabled = true;
		tutorialPanel.ShowVerticalAnim();

		yield return new WaitUntil(() => inputManager.VerticalInput < 0.2f);
		yield return new WaitUntil(() => inputManager.VerticalInput > 0.8f);
		
		AnalyticsSystem.RecordTutorialEvent(2);

		AudioSystem.Play(successClip);
		tutorialPanel.HideText();
		tutorialPanel.HideVerticalAnim();
		yield return new WaitForSeconds(1f);
		
		tutorialPanel.ShowText(tutorialTexts[2]);
		
		yield return new WaitForSeconds(1f);
		tutorialPanel.ShowHorizontalAnim();
		
		yield return new WaitUntil(() => inputManager.HorizontalInput < 0.3f);
		yield return new WaitUntil(() => inputManager.HorizontalInput > 0.7f);
		
		AnalyticsSystem.RecordTutorialEvent(3);
		
		AudioSystem.Play(successClip);
		tutorialPanel.HideText();
		tutorialPanel.HideHorizontalAnim();
		yield return new WaitForSeconds(1f);
		
		tutorialPanel.ShowText(tutorialTexts[3]);
		yield return new WaitForSeconds(1f);
		advance = false;
		tutorialPanel.ShowNextButton(() => {
			advance = true;
		});
		
		yield return new WaitUntil(() => advance);
		
		AnalyticsSystem.RecordTutorialEvent(4);
		
		UIController.Instance.FadeInToBlack(() => {
			PlayerPrefsManager.UserData.isTutorialDone = true;
			PlayerPrefsManager.SaveUserData();
			SceneManager.LoadScene("Game");
		});
	}

	private void Update() {
		userCar.UpdateCar(inputManager.VerticalInput, inputManager.HorizontalInput);
	}
}