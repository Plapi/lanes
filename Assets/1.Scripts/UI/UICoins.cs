using System;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class UICoins : UIObject {
	
	[SerializeField] private TextMeshProUGUI coinsText;
	[SerializeField] private TextMeshProUGUI incomeText;
	[SerializeField] private Transform m_incomeProgress;
	[SerializeField] private UICoinsIncomeAnim incomeAnim;

	public void UpdateCoins(int coins, int income) {
		coinsText.text = coins.ToString("N0");
		incomeText.text = income.ToString("N0");
	}

	public void UpdateProgress(float progress) {
		m_incomeProgress.SetScaleX(progress);
	}

	public void PlayCoinsIncomeAnim(Action onComplete = null) {
		incomeAnim.Play(() => {
			coinsText.transform.DOPunchScale(Vector3.one * 0.5f, 0.2f).SetEase(Ease.OutBounce);
			coinsText.DOColor(Color.green, 0.25f).OnComplete(() => {
				coinsText.DOColor(Color.white, 0.25f);
			});
			onComplete?.Invoke();
		});
	}
}
