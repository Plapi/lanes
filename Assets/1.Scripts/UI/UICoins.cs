using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UICoins : UIObject {
	
	[SerializeField] private TextMeshProUGUI coinsText;
	[SerializeField] private TextMeshProUGUI incomeText;
	[SerializeField] private Transform incomeProgress;
	[SerializeField] private UICoinsIncomeAnim incomeAnim;
	[SerializeField] private TextMeshProUGUI vaultFull;

	private Color vaultNotFullColor;
	
	private void Awake() {
		vaultNotFullColor = incomeProgress.GetComponent<Image>().color;
	}

	public void UpdateCoins(int coins, int income) {
		coinsText.text = Utils.FormatInt(coins);
		incomeText.text = $"+{Utils.FormatInt(income)}";
		UpdateLayout();
		bool vaultIsFull = PlayerPrefsManager.UserData.VaultIsFull();
		if (vaultIsFull != vaultFull.gameObject.activeSelf) {
			vaultFull.gameObject.SetActive(vaultIsFull);
			incomeProgress.GetComponent<Image>().color = vaultIsFull ? vaultFull.color : vaultNotFullColor;
		}
	}

	public void UpdateCoins(int coins) {
		coinsText.text = Utils.FormatInt(coins);
		UpdateLayout();
	}

	public void ConsumeCoins(int coins) {
		coinsText.text = Utils.FormatInt(coins);
		PlayConsumeCoinsAnim();
		UpdateLayout();
	}

	public void PlayConsumeCoinsAnim() {
		PlayUpdateCoinsAnim(Color.red);
	}

	public void PlayReceiveCoinsAnim() {
		PlayUpdateCoinsAnim(Color.green);
	}

	public void PlayUpdateCoinsAnim(Color color) {
		DOTween.Kill(coinsText);
		coinsText.DOColor(color, 0.25f).OnComplete(() => {
			coinsText.DOColor(Color.white, 0.25f);
		});
	}

	public void UpdateProgress(float progress) {
		incomeProgress.SetScaleX(progress);
	}

	public void PlayCoinsIncomeAnim(Action onComplete = null) {
		incomeAnim.Play(() => {
			coinsText.transform.DOPunchScale(Vector3.one * 0.5f, 0.2f).SetEase(Ease.OutBounce);
			DOTween.Kill(coinsText);
			PlayReceiveCoinsAnim();
			onComplete?.Invoke();
		});
	}

	private void UpdateLayout() {
		GameController.Instance.EndOfFrame(() => {
			HorizontalLayoutGroup horizontalLayoutGroup = coinsText.transform.parent.GetComponent<HorizontalLayoutGroup>();
			horizontalLayoutGroup.enabled = false;
			horizontalLayoutGroup.enabled = true;
		});
	}
}
