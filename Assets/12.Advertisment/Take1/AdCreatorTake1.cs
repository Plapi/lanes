using System.Collections;
using UnityEngine;

public class AdCreatorTake1 : MonoBehaviour {
	
	private static readonly int VictoryTriggerId = Animator.StringToHash("Victory");

	[SerializeField] private Animator mainCharacter;
	[SerializeField] private UITutorialSpeechBubble[] speechBubbles;
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private AudioClip[] voiceSounds;
	
	private void Awake() {
		PlayerPrefsManager.UserData.companyTutorialIsDone = true;
		PlayerPrefsManager.UserData.drivingTutorialIsDone = true;
		FindObjectsByType<CameraController>(FindObjectsSortMode.None)[0].gameObject.SetActive(false);
	}

	private IEnumerator Start() {
		speechBubbles[0].gameObject.SetActive(false);
		
		yield return new WaitForSeconds(1f);
		
		UIController.Instance.FadeInToBlack(() => {
			UIController.Instance.FadeOutFromBlack();
		});
		
		yield return new WaitForSeconds(1f);
		
		speechBubbles[0].Show();
		audioSource.clip = voiceSounds[0];
		audioSource.Play();
		
		yield return new WaitForSeconds(6f);
		
		mainCharacter.SetTrigger(VictoryTriggerId);
		speechBubbles[0].Hide();
		speechBubbles[1].Show();
		audioSource.clip = voiceSounds[1];
		audioSource.Play();
		
		yield return new WaitForSeconds(6f);
		
		UIController.Instance.FadeInToBlack();
	}
	
}
