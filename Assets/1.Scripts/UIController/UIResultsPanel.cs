using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using TMPro;
using UnityEngine.PlayerLoop;
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
        personText.text = data.persons.Count.ToString();
        personText.transform.GetChild(0).gameObject.SetActive(data.personBest);
        UpdatePersons(data.persons, personText.transform.parent.GetChild(0));
        coinsText.text = Utils.FormatInt(data.coins);
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
        ribbon.SetAnchorPosY(150f);
        ribbon.DOAnchorPosY(ribbonY, UIController.defaultTime).SetEase(Ease.OutQuad).SetUpdate(true);
        
        stats.gameObject.SetActive(true);
        CanvasGroup[] children = stats.GetComponentsInChildren<CanvasGroup>();
        for (int i = 0; i < children.Length; i++) {
            children[i].alpha = 0f;
        }
        
        yield return Utils.WaitForRealTime(0.15f);
        ribbon.DOPunchScale(Vector3.one * 0.2f, UIController.defaultTime).SetUpdate(true);
        
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
        if (data.coins > 0 && AdsController.HasInstance() && AdsController.Instance.CanShowAd()) {
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

    private static void UpdatePersons(List<int> persons, Transform container) {

        List<int> distinctPersons = new();
        List<int> personCounts = new();
        for (int i = 0; i < persons.Count; i++) {
            int index = distinctPersons.FindIndex(p => p == persons[i]);
            if (index == -1) {
                distinctPersons.Add(persons[i]);
                personCounts.Add(1);
            } else {
                personCounts[index]++;
            }
        }
        
        int max = Mathf.Max(distinctPersons.Count, container.childCount);
        for (int i = 0; i < max; i++) {
            if (i >= container.childCount) {
                Instantiate(container.GetChild(0).gameObject, container);
            }
            container.GetChild(i).gameObject.SetActive(i < distinctPersons.Count);
        }
        for (int i = 0; i < distinctPersons.Count; i++) {
            container.GetChild(i).GetComponent<Image>().sprite = Settings.Instance.personSprites[distinctPersons[i]];
            TextMeshProUGUI text = container.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>();
            text.gameObject.SetActive(personCounts[i] > 1);
            if (personCounts[i] > 1) {
                text.text = $"x{personCounts[i]}";
            }
        }
    }

    public new class Data: UIPanelBase.Data {
        public int distance;
        public List<int> persons;
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
                persons = new List<int> { 2, 0, 5, 5, 2},
                coins = Random.Range(200, 2000),
                distanceBest = true,
                personBest = true
            });
            resultsPanel.Show();
        }
        if (GUILayout.Button("Show Without Coins")) {
            resultsPanel.Init(new UIResultsPanel.Data {
                distance = Random.Range(100, 1000),
                persons = new List<int>(),
                coins = 0,
                distanceBest = false,
                personBest = false
            });
            resultsPanel.Show();
        }
    }
}
#endif