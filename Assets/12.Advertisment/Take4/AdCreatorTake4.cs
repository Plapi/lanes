using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class AdCreatorTake4 : MonoBehaviour {

	[SerializeField] private VideoPlayer[] videoPlayers;
	[SerializeField] private RectTransform transcriptRect;
	[SerializeField] private TextMeshProUGUI transcriptText;
	[SerializeField] private string[] transcriptTexts;
	[SerializeField] private float[] transcriptTimes;
	[SerializeField] private AudioSource audioSource;
	
	[SerializeField] private string[] transcriptTexts1;
    [SerializeField] private float[] transcriptTimes1;
    
    [SerializeField] private string[] transcriptTexts2;
    [SerializeField] private float[] transcriptTimes2;
    
    [SerializeField] private string[] transcriptTexts3;
    [SerializeField] private float[] transcriptTimes3;
    
    [SerializeField] private string[] transcriptTexts4;
    [SerializeField] private float[] transcriptTimes4;
    
    [SerializeField] private AudioClip[] audioClips;

	private IEnumerator Start() {
		transcriptRect.gameObject.SetActive(false);
		for (int i = 0; i < videoPlayers.Length; i++) {
			videoPlayers[i].Play();
			videoPlayers[i].Pause();	
		}
		
		yield return new WaitForSeconds(1f);

		PlayVideos();
		
		yield return new WaitForSeconds(1.5f);
		
		audioSource.clip = audioClips[0];
		PlayTranscripts(transcriptTexts, transcriptTimes);

		yield return new WaitForSeconds(transcriptTimes[^1] + 0.5f);
		transcriptRect.gameObject.SetActive(false);
		
		yield return new WaitForSeconds(0.5f);
		
		audioSource.clip = audioClips[1];
		PlayTranscripts(transcriptTexts1, transcriptTimes1);
		yield return new WaitForSeconds(transcriptTimes1[^1] + 0.5f);
		transcriptRect.gameObject.SetActive(false);
		
		yield return new WaitForSeconds(1.6f);
		
		audioSource.clip = audioClips[2];
		PlayTranscripts(transcriptTexts2, transcriptTimes2);
		yield return new WaitForSeconds(transcriptTimes2[^1] + 0.5f);
		transcriptRect.gameObject.SetActive(false);
		
		yield return new WaitForSeconds(0.8f);
		
		audioSource.clip = audioClips[3];
		PlayTranscripts(transcriptTexts3, transcriptTimes3);
		yield return new WaitForSeconds(transcriptTimes3[^1] + 0.5f);
		
		audioSource.clip = audioClips[4];
		PlayTranscripts(transcriptTexts4, transcriptTimes4);
		yield return new WaitForSeconds(transcriptTimes4[^1] + 0.5f);
		transcriptRect.gameObject.SetActive(false);
	}

	private void PlayVideos(int index = 0) {
		if (index == videoPlayers.Length) {
			return;
		}
		if (index > 0) {
			videoPlayers[index - 1].gameObject.SetActive(false);
		}
		videoPlayers[index].gameObject.SetActive(true);
		videoPlayers[index].Play();
		videoPlayers[index].loopPointReached += _ => {
			PlayVideos(index + 1);
		};
	}
	
	private void PlayTranscripts(string[] transcriptTexts, float[] transcriptTimes) {
		audioSource.Play();
		transcriptRect.gameObject.SetActive(true);
		transcriptText.text = transcriptTexts[0];
		for (int i = 0; i < transcriptTexts.Length; i++) {
			int ii = i;
			this.Wait(transcriptTimes[i], () => {
				transcriptText.text = transcriptTexts[ii];
				transcriptText.ForceMeshUpdate();
				this.EndOfFrame(() => {
					HorizontalLayoutGroup horizontalLayoutGroup = transcriptRect.GetComponent<HorizontalLayoutGroup>();
					horizontalLayoutGroup.enabled = false;
					horizontalLayoutGroup.enabled = true;
				});
			});
		}
	}
}
