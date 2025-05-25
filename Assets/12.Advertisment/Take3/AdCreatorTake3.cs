using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdCreatorTake3 : MonoBehaviour {

	[SerializeField] private Animator animator;
	[SerializeField] private RectTransform transcriptRect;
	[SerializeField] private TextMeshProUGUI transcriptText;
	[SerializeField] private string[] transcriptTexts;
	[SerializeField] private float[] transcriptTimes;
	[SerializeField] private AudioSource audioSource;
	
	private void Update() {
		if (Input.GetKeyDown(KeyCode.A)) {
			
			StartCoroutine(Transcripts());
		}
	}
	
	private IEnumerator Transcripts() {
		yield return new WaitForSeconds(0.2f);

		animator.enabled = true;
		
		audioSource.Play();
		transcriptRect.gameObject.SetActive(true);
		
		for (int i = 0; i < transcriptTexts.Length; i++) {
			transcriptText.text = transcriptTexts[i];
			this.EndOfFrame(() => {
				HorizontalLayoutGroup horizontalLayoutGroup = transcriptRect.GetComponent<HorizontalLayoutGroup>();
				horizontalLayoutGroup.enabled = false;
				horizontalLayoutGroup.enabled = true;
			});
			yield return new WaitForSeconds(transcriptTimes[i]);
		}
		transcriptRect.gameObject.SetActive(false);
	}

}
