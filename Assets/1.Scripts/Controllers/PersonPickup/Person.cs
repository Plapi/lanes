using UnityEngine;

public class Person : MonoBehaviour {
	
	[SerializeField] private Animator animator;
	
	private static readonly int wavingTriggerId = Animator.StringToHash("Waving");
	private static readonly int thankfulTriggerId = Animator.StringToHash("Thankful");

	public void SetWaving(out int personIndex) {
		gameObject.SetActive(true);
		animator.SetTrigger(wavingTriggerId);
		personIndex = SetRandomMesh();
	}

	public void SetThankful() {
		gameObject.SetActive(true);
		animator.SetTrigger(thankfulTriggerId);
	}

	private int SetRandomMesh() {
		int randomIndex = Random.Range(0, transform.childCount - 1);
		for (int i = 0; i < transform.childCount - 1; i++) {
			transform.GetChild(i).gameObject.SetActive(randomIndex == i);
		}
		return randomIndex;
	}
}
