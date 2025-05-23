using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class AdCreator : MonoBehaviour {
	
	private static readonly int animatorSpeedId = Animator.StringToHash("Speed");
	private static readonly int animatorThinkingId = Animator.StringToHash("Thinking");

	[SerializeField] private new Transform camera;
	[SerializeField] private Animator mainCharacter;

	[Space] 
	[SerializeField] private TransPoint[] cameraPoints;
	[SerializeField] private TransPoint[] mainCharacterPoints;
	
	[Space]
	[SerializeField] private UITutorialSpeechBubble[] mainCharacterSpeechBubbles;
	
	private IEnumerator Start() {

		mainCharacter.SetFloat(animatorSpeedId, 2f);
		StartCoroutine(ExecuteTransPoint(mainCharacter.transform, mainCharacterPoints));

		mainCharacterPoints[0].onMoveComplete = () => {
			mainCharacter.SetFloat(animatorSpeedId, 0f);
			mainCharacter.SetTrigger(animatorThinkingId);
			mainCharacterSpeechBubbles[0].Show();
		};
		mainCharacterPoints[1].onMoveStart = () => {
			mainCharacter.SetFloat(animatorSpeedId, 2f);
		};
					
		yield return new WaitForSeconds(1.5f);
		
		StartCoroutine(ExecuteTransPoint(camera, cameraPoints));

		cameraPoints[1].onMoveStart = () => {
			mainCharacterSpeechBubbles[0].Hide();
		};
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
