using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using TMPro;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIMissionResultsPanel : UIPanel<UIMissionResultsPanel.Data> {

	[Space]
	[SerializeField] private RectTransform ribbon;
	[SerializeField] private RectTransform[] stars;
	[SerializeField] private RectTransform stats;
	
	[Space]
	[SerializeField] private TextMeshProUGUI distanceText;
	[SerializeField] private TextMeshProUGUI coinsText;
	
	[Space]
	[SerializeField] private Button adCollectButton;
	[SerializeField] private Button collectButton;
	
	[Space]
	[SerializeField] private AudioClip celebrateClip;
	[SerializeField] private AudioClip statsClip;
	[SerializeField] private AudioClip collectsClip;
	
	protected override void OnInit() {

		for (int i = 0; i < stars.Length; i++) {
			stars[i].GetChild(0).gameObject.SetActive(data.stars <= i);
			stars[i].GetChild(1).gameObject.SetActive(data.stars > i);
		}
		
		distanceText.text = $"{data.distance:N0} m";
		distanceText.transform.GetChild(0).gameObject.SetActive(data.distanceBest);
		
		coinsText.text = Utils.FormatInt(data.item.coins);
		adCollectButton.onClick.RemoveAllListeners();
		adCollectButton.onClick.AddListener(data.onAdCollect);
		collectButton.onClick.RemoveAllListeners();
		collectButton.onClick.AddListener(data.onCollect);
	}
	
	protected override void ShowAnim(Action onComplete) {
		OnShowAnimBegin?.Invoke();
		gameObject.SetActive(true);
		stats.gameObject.SetActive(false);
		adCollectButton.gameObject.SetActive(false);
		collectButton.gameObject.SetActive(false);
		AudioSystem.Play(celebrateClip);
		StartCoroutine(ShowAnimIEnumerator(() => {
			OnShowAnimEnd?.Invoke();
			onComplete?.Invoke();
		}));
	}
	
	private IEnumerator ShowAnimIEnumerator(Action onComplete) {
        
        CanvasGroup backgroundCanvasGroup = background.GetComponent<CanvasGroup>();
        backgroundCanvasGroup.alpha = 0f;
        backgroundCanvasGroup.DOFade(1f, UIController.defaultTime).SetUpdate(true);
        
        float ribbonY = ribbon.anchoredPosition.y;
        ribbon.SetAnchorPosY(350f);
        ribbon.DOAnchorPosY(ribbonY, UIController.defaultTime).SetEase(Ease.OutQuad).SetUpdate(true);
        
        stats.gameObject.SetActive(true);
        CanvasGroup[] children = stats.GetComponentsInChildren<CanvasGroup>();
        for (int i = 0; i < children.Length; i++) {
            children[i].alpha = 0f;
        }
        
        yield return Utils.WaitForRealTime(0.15f);
        // ribbon.DOPunchScale(Vector3.one * 0.2f, UIController.defaultTime).SetUpdate(true);
        
        children[1].gameObject.SetActive(false);
        children[1].gameObject.SetActive(true);
        
        for (int i = 0; i < children.Length; i++) {
            yield return Utils.WaitForRealTime(i == 0 ? 0.2f : 0.5f);
            AudioSystem.Play(statsClip);
            RectTransform rectTransform = children[i].GetComponent<RectTransform>();
            float toY = rectTransform.anchoredPosition.y;
            rectTransform.SetAnchorPosY(toY + 100f);
            rectTransform.DOAnchorPosY(toY, 0.4f).SetEase(Ease.OutQuad).SetUpdate(true);
            children[i].DOFade(1f, 0.4f).SetUpdate(true);
        }
        
        yield return Utils.WaitForRealTime(0.2f);

        List<RectTransform> collectRects = new();
        if (AdsController.HasInstance() && AdsController.Instance.CanShowAd()) {
	        collectRects.Add(adCollectButton.GetComponent<RectTransform>());
        }
        collectRects.Add(collectButton.GetComponent<RectTransform>());
        AudioSystem.Play(collectsClip);
        
        for (int i = 0; i < collectRects.Count; i++) {
            collectRects[i].gameObject.SetActive(true);
            float toY = collectRects[i].anchoredPosition.y;
            collectRects[i].SetAnchorPosY(-300f);
            collectRects[i].DOAnchorPosY(toY, 0.4f).SetEase(Ease.OutQuad).SetUpdate(true);
            yield return Utils.WaitForRealTime(0.2f);
        }

        onComplete();
    }

	public new class Data: UIPanelBase.Data {
		public UIMissionsList.ItemData item;
		public int stars;
		public int distance;
		public bool distanceBest;
		public UnityAction onAdCollect;
		public UnityAction onCollect;
	}
	
}

#if UNITY_EDITOR
[CustomEditor(typeof(UIMissionResultsPanel))]
public class UIMissionResultsPanelEditor : Editor {
	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		
		UIMissionResultsPanel resultsPanel = (UIMissionResultsPanel)target;
		
		GUILayout.Space(10f);
		if (GUILayout.Button("Show")) {
			resultsPanel.Init(new UIMissionResultsPanel.Data {
				item = new UIMissionsList.ItemData {
					person = new RideController.CurrentPerson {
						group = 1,
						index = 12
					},
					coins = 5000,
					intersections = 4
				},
				stars = 2,
				distance = Random.Range(1000, 10000),
				distanceBest = true
			});
			resultsPanel.Show();
		}
	}
}
#endif
