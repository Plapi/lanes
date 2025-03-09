using System;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UICoinsAnim : UIObject {

	[SerializeField] [Range(0f, 1f)] private float debugTime;
	[SerializeField] private List<RectTransform> coins;
	[SerializeField] private RectTransform[] controlPoints;
	[SerializeField] private bool drawGizmos;
	[SerializeField] private AnimationCurve scaleCurve;
	[SerializeField] private Ease ease;

	[Space]
	[SerializeField] private AudioClip coinSoundIn;
	[SerializeField] private AudioClip coinSoundOut;
	
	private Vector3 initCoinLocalPos;
	private float initCP1X;
	private float initCP2X;

	private void Awake() {
		initCoinLocalPos = coins[0].localPosition;
		initCP1X = controlPoints[1].localPosition.x;
		initCP2X = controlPoints[2].localPosition.x;
	}

	public void Play(int coinsCount, Action onComplete = null) {
		StartCoroutine(PlayIEnumerator(coinsCount, onComplete));
	}

	private IEnumerator PlayIEnumerator(int coinsCount, Action onComplete = null) {
		for (int i = coins.Count - 1; i < coinsCount; i++) {
			RectTransform coin = Instantiate(coins[^1].gameObject, coins[^1].transform.parent)
				.GetComponent<RectTransform>();
			coin.localPosition = initCoinLocalPos;
			coin.name = $"Coin{i}";
			coins.Add(coin);
		}
		for (int i = 0; i < coins.Count; i++) {
			Image image = coins[i].GetComponent<Image>();
			image.DOKill();
			image.SetAlpha(1f);
			coins[i].gameObject.SetActive(i < coinsCount);
			coins[i].localPosition = initCoinLocalPos;
		}
		WaitForSeconds waitForSeconds = new WaitForSeconds(0.1f);
		for (int i = 0; i < coinsCount; i++) {
			float time = 0f;
			int coinIndex = i;

			Vector3 cp0 = controlPoints[0].localPosition;
			Vector3 cp1 = controlPoints[1].localPosition;
			Vector3 cp2 = controlPoints[2].localPosition;
			Vector3 cp3 = controlPoints[3].localPosition;
			cp1.x = Random.Range(-30f, 30f) + initCP1X;
			cp2.x = Random.Range(-30f, 30f) + initCP2X;

			this.Wait(0.2f, () => {
				AudioSystem.Play(coinSoundIn);
			});
			
			this.Wait(1.7f, () => {
				AudioSystem.Play(coinSoundOut);
			});
			
			DOTween.To(() => time, x => time = x, 1f, 1.5f)
				.SetEase(ease)
				.OnUpdate(() => {
					coins[coinIndex].localPosition = Bezier.GetPoint(cp0, cp1, cp2, cp3, time);
					coins[coinIndex].localScale = Vector3.one * scaleCurve.Evaluate(time);
				}).OnComplete(() => {
					coins[coinIndex].DOPunchScale(Vector3.one * 0.2f, UIController.defaultTime);
					coins[coinIndex].GetComponent<Image>().DOFade(0f, 0.4f).OnComplete(() => {
						coins[coinIndex].gameObject.SetActive(false);
					});
					if (coinIndex == coinsCount - 1) {
						onComplete?.Invoke();
					}
				});
			
			yield return waitForSeconds;
		}
	}

	private void OnDrawGizmos() {
		if (!drawGizmos) {
			return;
		}
		Gizmos.color = Color.red;
		for (int i = 0; i < controlPoints.Length; i++) {
			Gizmos.DrawSphere(controlPoints[i].position, 10f);
		}
		for (float t = 0f; t < 1f; t += 0.001f) {
			Vector3 point0 = GetBezierPoint(t);
			Vector3 point1 = GetBezierPoint(t + 0.001f);
			Gizmos.DrawLine(point0, point1);
		}
		if (!Application.isPlaying) {
			coins[0].localPosition = GetBezierLocalPoint(debugTime);
			coins[0].localScale = Vector3.one * scaleCurve.Evaluate(debugTime);
		}
	}

	private Vector3 GetBezierLocalPoint(float t) {
		return Bezier.GetPoint(controlPoints[0].localPosition, controlPoints[1].localPosition,
			controlPoints[2].localPosition, controlPoints[3].localPosition, t);
	}

	private Vector3 GetBezierPoint(float t) {
		return Bezier.GetPoint(controlPoints[0].position, controlPoints[1].position, controlPoints[2].position,
			controlPoints[3].position, t);
	}
}