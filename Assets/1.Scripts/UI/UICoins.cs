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
		coinsText.text = coins.ToString("N0");
		incomeText.text = $"+{income:N0}";
		UpdateLayout();
		bool vaultIsFull = PlayerPrefsManager.UserData.VaultIsFull();
		if (vaultIsFull != vaultFull.gameObject.activeSelf) {
			vaultFull.gameObject.SetActive(vaultIsFull);
			incomeProgress.GetComponent<Image>().color = vaultIsFull ? vaultFull.color : vaultNotFullColor;
		}
	}

	public void ConsumeCoins(int coins) {
		coinsText.text = coins.ToString("N0");
		DOTween.Kill(coinsText);
		coinsText.DOColor(Color.red, 0.25f).OnComplete(() => {
			coinsText.DOColor(Color.white, 0.25f);
		});
		UpdateLayout();
	}

	public void UpdateProgress(float progress) {
		incomeProgress.SetScaleX(progress);
	}

	public void PlayCoinsIncomeAnim(Action onComplete = null) {
		incomeAnim.Play(() => {
			coinsText.transform.DOPunchScale(Vector3.one * 0.5f, 0.2f).SetEase(Ease.OutBounce);
			DOTween.Kill(coinsText);
			coinsText.DOColor(Color.green, 0.25f).OnComplete(() => {
				coinsText.DOColor(Color.white, 0.25f);
			});
			onComplete?.Invoke();
		});
	}

	private void UpdateLayout() {
		this.EndOfFrame(() => {
			HorizontalLayoutGroup horizontalLayoutGroup = coinsText.transform.parent.GetComponent<HorizontalLayoutGroup>();
			horizontalLayoutGroup.enabled = false;
			horizontalLayoutGroup.enabled = true;
		});
	}
}
