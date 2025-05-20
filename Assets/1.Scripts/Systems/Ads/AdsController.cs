using System;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsController : MonoBehaviourSingleton<AdsController>, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener {

	[SerializeField] private string androidGameId;
	[SerializeField] private string iOSGameId;

	[Space] 
	[SerializeField] private string androidAdUnitId = "Interstitial_Android";
	[SerializeField] private string iOSAdUnitId = "Interstitial_iOS";

	private Status status;
	private string gameId;
	private string addUnitId;
	
	private Action onCompleteInit;
	private Action<bool> onCompleteShowAd;

	private string analyticsSource;

	public bool WasInitSuccessful { get; private set; }

	protected override void Awake() {
		base.Awake();
		DontDestroyOnLoad(this);
	}

	public void Init(Action onComplete) {
		onCompleteInit = onComplete;
		gameId = Application.platform == RuntimePlatform.IPhonePlayer ? iOSGameId : androidGameId;
		addUnitId = Application.platform == RuntimePlatform.IPhonePlayer ? iOSAdUnitId : androidAdUnitId;
		if (!Advertisement.isInitialized && Advertisement.isSupported) {
			Advertisement.Initialize(gameId, Settings.Instance.testMode, this);
		} else {
			onCompleteInit?.Invoke();
			onCompleteInit = null;
		}
	}

	public void OnInitializationComplete() {
		status = Status.InitializedSuccess;
		WasInitSuccessful = true;
		onCompleteInit?.Invoke();
		onCompleteInit = null;
	}

	public void OnInitializationFailed(UnityAdsInitializationError error, string message) {
		Debug.LogError($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
		status = Status.InitializedFail;
		onCompleteInit?.Invoke();
		onCompleteInit = null;
	}

	public void LoadAd() {
		if (status == Status.InitializedSuccess) {
			Advertisement.Load(addUnitId, this);
		}
	}

	public bool CanShowAd() {
		return status == Status.AdLoaded;
	}
	
	public void ShowAd(Action<bool> onComplete, string source) {
		AnalyticsSystem.RecordWatchAdStartEvent(source);
		if (Application.isEditor) {
			AnalyticsSystem.RecordWatchAdCompleteEvent(source);
			onComplete?.Invoke(true);
			return;
		}
		if (CanShowAd()) {
			onCompleteShowAd = onComplete;
			analyticsSource = source;
			Advertisement.Show(addUnitId, this);
		} else {
			onComplete?.Invoke(false);
		}
	}
	
	public void OnUnityAdsAdLoaded(string adUnitId) {
		status = Status.AdLoaded;
	}
	
	public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message) {
		Debug.LogError($"Error loading Ad Unit: {adUnitId} - {error.ToString()} - {message}");
		this.Wait(1f, LoadAd);
	}
	
	public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message) {
		Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
		onCompleteShowAd?.Invoke(false);
		onCompleteShowAd = null;
		status = Status.InitializedSuccess;
		this.Wait(1f, LoadAd);
	}
	
	public void OnUnityAdsShowStart(string adUnitId) { }
	public void OnUnityAdsShowClick(string adUnitId) { }

	public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState) {
		onCompleteShowAd?.Invoke(showCompletionState == UnityAdsShowCompletionState.COMPLETED);
		onCompleteShowAd = null;
		status = Status.InitializedSuccess;
		AnalyticsSystem.RecordWatchAdCompleteEvent(analyticsSource);
		this.Wait(1f, LoadAd);
	}
	
	private enum Status {
		None,
		InitializedSuccess,
		InitializedFail,
		AdLoaded
	}
}