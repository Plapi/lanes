using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using DG.Tweening;
using Random = UnityEngine.Random;

public class UICoinsIncomeAnim : UIObject {

	[SerializeField] private RectTransform coinsContainer;
	[SerializeField] private List<RectTransform> coins;
	[SerializeField] private RectTransform from;
	[SerializeField] private RectTransform to;
	[SerializeField] private Vector2[] outPositions;
	[SerializeField] private AudioClip showCoinClip;
	[SerializeField] private AudioClip endCoinClip;

	private Action onComplete;
	
	public void Play(Action onComplete) {
		StartCoroutine(PlayIEnumerator(onComplete));
	}

	private void OnDisable() {
		if (onComplete != null) {
			for (int i = 0; i < coins.Count; i++) {
				coins[i].DOKill();
				coins[i].gameObject.SetActive(false);
			}
			onComplete?.Invoke();
			onComplete = null;
		}
	}

	private IEnumerator PlayIEnumerator(Action onComplete) {
		this.onComplete = onComplete;
		
		int coinsCount = outPositions.Length;
		for (int i = coins.Count; i < coinsCount; i++) {
			RectTransform coin = Instantiate(coins[^1].gameObject, coins[^1].transform.parent).GetComponent<RectTransform>();
			coin.name = $"Coin{i}";
			coins.Add(coin);
		}

		transform.position = from.position;
		coinsContainer.SetLocalAngleZ(Random.Range(0, 360));
		
		Utils.ShuffleArray(outPositions);
		for (int i = 0; i < coins.Count; i++) {
			coins[i].anchoredPosition = Vector2.zero;
			coins[i].SetAngleZ(0f);
			coins[i].gameObject.SetActive(true);
			coins[i].SetScale(0.1f);
			coins[i].DOScale(Vector3.one, 0.3f).SetEase(Ease.OutExpo);
			coins[i].DOAnchorPos(outPositions[i] * 55f, 0.3f).SetEase(Ease.OutExpo);
			AudioSystem.Play(showCoinClip, MixerType.Effects, null, 0.3f);
			yield return new WaitForSeconds(Random.Range(0.02f, 0.1f));
		}
		
		yield return new WaitForSeconds(0.2f);

		for (int i = 0; i < coins.Count; i++) {
			int index = i;
			coins[i].DOMove(to.position, 0.5f).SetEase(Ease.OutCubic).OnComplete(() => {
				coins[index].gameObject.SetActive(false);
				if (index == coins.Count - 1) {
					to.DOPunchScale(Vector3.one * 0.1f, 0.2f).SetEase(Ease.OutBounce);
					AudioSystem.Play(endCoinClip, MixerType.Effects, null, 0.3f);
					this.onComplete?.Invoke();
					this.onComplete = null;
				}
			});
		}
	}
}
