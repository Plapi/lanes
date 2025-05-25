using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class AdCreatorTakeFinal : MonoBehaviour {

	[SerializeField] private VideoPlayer videoPlayer;
	[SerializeField] private VideoClip[] videoClips;
	[SerializeField] private AudioSource audioSource;

	private IEnumerator Start() {
		yield return new WaitForSeconds(1f);
		audioSource.Play();
		PlayVideos();
	}
	
	private void PlayVideos(int index = 0) {
		if (index == videoClips.Length) {
			return;
		}
		videoPlayer.clip = videoClips[index];
		videoPlayer.Play();
		videoPlayer.loopPointReached += _ => {
			PlayVideos(index + 1);
		};
	}
}
