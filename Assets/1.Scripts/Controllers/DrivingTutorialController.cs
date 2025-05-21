using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DrivingTutorialController : MonoBehaviour {

	[SerializeField] private Camera mainCamera;
	[SerializeField] private InputManager inputManager;
	[SerializeField] private TrackGenerator trackGenerator;
	[SerializeField] private UserCar userCar;
	[SerializeField] private AudioClip successClip;

	[Space]
	[SerializeField] private GameObject drivingInput;
	
	[Space]
	[SerializeField] private List<UITutorialSpeechBubble> speechBubbles;
	[SerializeField] private Button tutorialButton;
	[SerializeField] private GameObject leftTutorialArrow;
	[SerializeField] private GameObject rightTutorialArrow;
	[SerializeField] private GameObject bottomTutorialArrow;
	[SerializeField] private GameObject topTutorialArrow;
	
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
		
		StartCoroutine(Tutorial());
	}

	private void ShowNextSpeechBubble() {
		speechBubbles[0].Hide();
		speechBubbles[1].Show();
		speechBubbles.RemoveAt(0);
	}

	private IEnumerator Tutorial() {

		AnalyticsSystem.RecordDriveTutorialEvent(0);
		
		yield return new WaitForSeconds(2f);
		
		userCar.GetComponent<AudioSource>().enabled = true;

		bool advance = false;
		tutorialButton.gameObject.SetActive(true);
		tutorialButton.onClick.AddListener(() => {
			tutorialButton.gameObject.SetActive(false);
			advance = true;
		});
		yield return new WaitUntil(() => advance);
		AnalyticsSystem.RecordDriveTutorialEvent(1);
		
		ShowNextSpeechBubble();
		
		yield return new WaitForSeconds(2f);
		
		inputManager.enabled = true;
		drivingInput.SetActive(true);
		
		bottomTutorialArrow.SetActive(true);
		yield return new WaitUntil(() => inputManager.VerticalInput < 0.2f);
		bottomTutorialArrow.SetActive(false);
		AudioSystem.Play(successClip);
		
		topTutorialArrow.SetActive(true);
		yield return new WaitUntil(() => inputManager.VerticalInput > 0.8f);
		topTutorialArrow.SetActive(false);
		AudioSystem.Play(successClip);
		
		AnalyticsSystem.RecordDriveTutorialEvent(2);

		ShowNextSpeechBubble();
		yield return new WaitForSeconds(1f);
		
		leftTutorialArrow.SetActive(true);
		yield return new WaitUntil(() => inputManager.HorizontalInput < 0.3f);
		leftTutorialArrow.SetActive(false);
		AudioSystem.Play(successClip);
		
		rightTutorialArrow.SetActive(true);
		yield return new WaitUntil(() => inputManager.HorizontalInput > 0.7f);
		rightTutorialArrow.SetActive(false);
		AudioSystem.Play(successClip);
		
		AnalyticsSystem.RecordDriveTutorialEvent(3);
		
		ShowNextSpeechBubble();
		yield return new WaitForSeconds(1f);
		
		advance = false;
		tutorialButton.gameObject.SetActive(true);
		yield return new WaitUntil(() => advance);
		tutorialButton.gameObject.SetActive(false);
		
		AnalyticsSystem.RecordDriveTutorialEvent(4);
		
		UIController.Instance.FadeInToBlack(() => {
			PlayerPrefsManager.UserData.drivingTutorialIsDone = true;
			PlayerPrefsManager.SaveUserData();
			GameController.GoToDrive = true;
			SceneManager.LoadScene("Company");
		});
	}

	private void Update() {
		userCar.UpdateCar(inputManager.VerticalInput, inputManager.HorizontalInput);
	}
}