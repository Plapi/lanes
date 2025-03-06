using UnityEngine;

public class Person : MonoBehaviour {
	
	[SerializeField] private Animator animator;
	
	private static readonly int wavingTriggerId = Animator.StringToHash("Waving");
	private static readonly int thankfulTriggerId = Animator.StringToHash("Thankful");

	public void SetWaving() {
		gameObject.SetActive(true);
		animator.SetTrigger(wavingTriggerId);
		SetRandomMesh();
	}

	public void SetThankful() {
		gameObject.SetActive(true);
		animator.SetTrigger(thankfulTriggerId);
	}

	private void SetRandomMesh() {
		int randomIndex = Random.Range(0, transform.childCount - 1);
		for (int i = 0; i < transform.childCount - 1; i++) {
			transform.GetChild(i).gameObject.SetActive(randomIndex == i);
		}
	}
}
