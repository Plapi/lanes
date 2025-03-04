using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using TMPro;

public class UIResultsPanel : UIPanel<UIResultsPanel.Data> {

    [SerializeField] private RectTransform ribbon;
    [SerializeField] private RectTransform stats;
    
    [Space]
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI personText;
    [SerializeField] private TextMeshProUGUI coinsText;
    
    [Space]
    [SerializeField] private Button adCollectButton;
    [SerializeField] private Button collectButton;
    
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
            RectTransform rectTransform = children[i].GetComponent<RectTransform>();
            float toY = rectTransform.anchoredPosition.y;
            rectTransform.SetAnchorPosY(toY + 100f);
            rectTransform.DOAnchorPosY(toY, 0.4f).SetEase(Ease.OutQuad).SetUpdate(true);
            children[i].DOFade(1f, 0.4f).SetUpdate(true);
        }
        
        yield return Utils.WaitForRealTime(0.2f);
        
        RectTransform[] collectRects = new RectTransform[2] {
            adCollectButton.GetComponent<RectTransform>(),
            collectButton.GetComponent<RectTransform>()
        };
        for (int i = 0; i < collectRects.Length; i++) {
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
