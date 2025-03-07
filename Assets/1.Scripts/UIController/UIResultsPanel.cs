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

public class UIResultsPanel : UIPanel<UIResultsPanel.Data> {

    [Space]
    [SerializeField] private RectTransform ribbon;
    [SerializeField] private RectTransform stats;
    
    [Space]
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI personText;
    [SerializeField] private TextMeshProUGUI coinsText;
    
    [Space]
    [SerializeField] private Button adCollectButton;
    [SerializeField] private Button collectButton;

    [Space]
    [SerializeField] private AudioClip celebrateClip;
    [SerializeField] private AudioClip statsClip;
    [SerializeField] private AudioClip collectsClip;
    
    protected override void OnInit() {
        distanceText.text = $"{data.distance:N0} m";
        distanceText.transform.GetChild(0).gameObject.SetActive(data.distanceBest);
        personText.text = data.persons.ToString();
        personText.transform.GetChild(0).gameObject.SetActive(data.personBest);
        coinsText.text = data.coins.ToString("N0");
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
        this.PlaySound(celebrateClip);
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
        ribbon.SetAnchorPosY(150f);
        ribbon.DOAnchorPosY(ribbonY, UIController.defaultTime).SetEase(Ease.OutQuad).SetUpdate(true);
        
        stats.gameObject.SetActive(true);
        CanvasGroup[] children = stats.GetComponentsInChildren<CanvasGroup>();
        for (int i = 0; i < children.Length; i++) {
            children[i].alpha = 0f;
        }
        
        yield return Utils.WaitForRealTime(0.15f);
        ribbon.DOPunchScale(Vector3.one * 0.2f, UIController.defaultTime).SetUpdate(true);
        
        for (int i = 0; i < children.Length; i++) {
            yield return Utils.WaitForRealTime(i == 0 ? 0.2f : 0.5f);
            this.PlaySound(statsClip);
            RectTransform rectTransform = children[i].GetComponent<RectTransform>();
            float toY = rectTransform.anchoredPosition.y;
            rectTransform.SetAnchorPosY(toY + 100f);
            rectTransform.DOAnchorPosY(toY, 0.4f).SetEase(Ease.OutQuad).SetUpdate(true);
            children[i].DOFade(1f, 0.4f).SetUpdate(true);
        }
        
        yield return Utils.WaitForRealTime(0.2f);

        List<RectTransform> collectRects = new();
        if (data.coins > 0) {
            collectRects.Add(adCollectButton.GetComponent<RectTransform>());
        }
        collectRects.Add(collectButton.GetComponent<RectTransform>());
        
        this.PlaySound(collectsClip);
        
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
        public int distance;
        public int persons;
        public int coins;
        public bool distanceBest;
        public bool personBest;
        public UnityAction onAdCollect;
        public UnityAction onCollect;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UIResultsPanel))]
public class UIResultsPanelEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
		
        UIResultsPanel resultsPanel = (UIResultsPanel)target;
		
        GUILayout.Space(10f);
        if (GUILayout.Button("Show With Coins")) {
            resultsPanel.Init(new UIResultsPanel.Data {
                distance = Random.Range(1000, 10000),
                persons = Random.Range(2, 6),
                coins = Random.Range(200, 2000),
                distanceBest = true,
                personBest = true
            });
            resultsPanel.Show();
        }
        if (GUILayout.Button("Show Without Coins")) {
            resultsPanel.Init(new UIResultsPanel.Data {
                distance = Random.Range(100, 1000),
                persons = 0,
                coins = 0,
                distanceBest = false,
                personBest = false
            });
            resultsPanel.Show();
        }
    }
}
#endif