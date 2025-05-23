using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class AdCreatorTake0 : MonoBehaviour {
	
	private static readonly int animatorSpeedId = Animator.StringToHash("Speed");
	private static readonly int animatorThinkingId = Animator.StringToHash("Thinking");

	[SerializeField] private new Transform camera;
	[SerializeField] private Animator mainCharacter;
	
	[Space] 
	[SerializeField] private TransPoint[] cameraPoints;
	
	[Space]
	[SerializeField] private UITutorialSpeechBubble[] mainCharacterSpeechBubbles;

	
	private float mainCharacterRotationY;

	private void Awake() {
		PlayerPrefsManager.UserData.companyTutorialIsDone = true;
		PlayerPrefsManager.UserData.drivingTutorialIsDone = true;
		FindObjectsByType<CameraController>(FindObjectsSortMode.None)[0].gameObject.SetActive(false);
		mainCharacterRotationY = mainCharacter.transform.localEulerAngles.y;
		mainCharacterSpeechBubbles[0].gameObject.SetActive(true);
		mainCharacterSpeechBubbles[0].gameObject.SetActive(false);
	}

	private IEnumerator MainCharacterTrans() {
		mainCharacter.SetFloat(animatorSpeedId, 1f);

		yield return new WaitForSeconds(5f);
		
		mainCharacter.SetTrigger(animatorThinkingId);
		
		yield return new WaitForSeconds(0.5f);
		mainCharacterSpeechBubbles[0].Show();
		camera.GetComponent<AudioSource>().Play();

		yield return new WaitForSeconds(3f);
		mainCharacterSpeechBubbles[0].Hide();
		
		float time = 0f;
		float initRotationY = mainCharacterRotationY;
		DOTween.To(() => time, x => time = x, 1f, 1.5f)
			.SetEase(Ease.Linear)
			.SetDelay(1f)
			.OnUpdate(() => {
				mainCharacterRotationY = Mathf.Lerp(initRotationY, 90f, time);
			});
		
		yield return new WaitForSeconds(1.5f);
		
		UIController.Instance.FadeInToBlack();
	}

	private IEnumerator Start() {

		StartCoroutine(MainCharacterTrans());
					
		yield return new WaitForSeconds(2f);
		
		StartCoroutine(ExecuteTransPoint(camera, cameraPoints));

		cameraPoints[1].onMoveStart = () => {
			mainCharacterSpeechBubbles[0].Hide();
		};
	}

	private void LateUpdate() {
		mainCharacter.transform.SetLocalAngleY(mainCharacterRotationY);
	}

	private static IEnumerator ExecuteTransPoint(Transform obj, TransPoint[] transPoints) {
		for (int i = 0; i < transPoints.Length; i++) {
			if (transPoints[i].moveDuration > 0f) {
				int index = i;
				transPoints[index].onMoveStart?.Invoke();
				obj.DOMove(transPoints[i].target.position, transPoints[i].moveDuration).SetEase(transPoints[i].moveEase).OnComplete(() => {
					transPoints[index].onMoveComplete?.Invoke();
				});
			}
			if (transPoints[i].rotateDuration > 0f) {
				obj.DORotate(transPoints[i].target.eulerAngles, transPoints[i].rotateDuration).SetEase(transPoints[i].rotateEase);
			}
			yield return new WaitForSeconds(Mathf.Max(transPoints[i].moveDuration, transPoints[i].rotateDuration) + transPoints[i].exitDelay);
		}
	}
	
	[Serializable]
	private class TransPoint {
		public Transform target;
		public float moveDuration;
		public float rotateDuration;
		public float exitDelay;
		public Ease moveEase;
		public Ease rotateEase;
		public Action onMoveStart;
		public Action onMoveComplete;
	}
}
