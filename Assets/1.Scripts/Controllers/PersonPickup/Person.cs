using UnityEngine;

public class Person : MonoBehaviour {
	
	private static readonly int wavingTriggerId = Animator.StringToHash("Waving");
	private static readonly int thankfulTriggerId = Animator.StringToHash("Thankful");
	
	private Animator animator;
	
	public void SetWaving(int group, int personIndex) {
		gameObject.SetActive(true);
		SetPerson(group, personIndex);
		animator.SetTrigger(wavingTriggerId);
	}

	public void SetThankful() {
		gameObject.SetActive(true);
		animator.SetTrigger(thankfulTriggerId);
	}

	private void SetPerson(int group, int index) {
		transform.GetChild(0).gameObject.SetActive(false);
		transform.GetChild(1).gameObject.SetActive(false);
		animator = transform.GetChild(group).GetComponent<Animator>();
		animator.gameObject.SetActive(true);
		HideAllCharacters(animator.transform, index);
	}
	
	private static void HideAllCharacters(Transform character, int exceptIndex) {
		for (int i = 0; i < character.childCount - 1; i++) {
			Transform child = character.GetChild(i);
			child.gameObject.SetActive(i == exceptIndex);
		}
	}
}
