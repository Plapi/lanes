using System;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsController : MonoBehaviourSingleton<AdsController>, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener {

	[SerializeField] private string androidGameId;
	[SerializeField] private string iOSGameId;

	private readonly AdData[] adsData = {
		new() {
			type = AdType.Interstitial_Android
		}, new() {
			type = AdType.Rewarded_Android
		}
	};
	
	private Status status;
	
	public bool WasInitSuccessful { get; private set; }

	protected override void Awake() {
		base.Awake();
		DontDestroyOnLoad(this);
	}

	public void Init() {
		string gameId = Application.platform == RuntimePlatform.IPhonePlayer ? iOSGameId : androidGameId;
		if (!Advertisement.isInitialized && Advertisement.isSupported) {
			Advertisement.Initialize(gameId, Settings.Instance.testMode, this);
		} else {
			LoadAds();
		}
	}

	public void OnInitializationComplete() {
		status = Status.InitializedSuccess;
		WasInitSuccessful = true;
		LoadAds();
	}

	public void OnInitializationFailed(UnityAdsInitializationError error, string message) {
		Debug.LogError($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
		status = Status.InitializedFail;
	}

	private void LoadAds() {
		if (status == Status.InitializedSuccess) {
			for (int i = 0; i < adsData.Length; i++) {
				Advertisement.Load(adsData[i].UnitId, this);
			}
		}
	}
	
	private void LoadAd(AdType type) {
		if (status == Status.InitializedSuccess) {
			Advertisement.Load(adsData[(int)type].UnitId, this);
		}
	}

	public bool CanShowAd() {
		return status == Status.AdLoaded;
	}
	
	public void ShowAd(AdType type, Action<bool> onComplete = null, string source = "") {
		AnalyticsSystem.RecordWatchAdStartEvent(source);
		if (Application.isEditor) {
			AnalyticsSystem.RecordWatchAdCompleteEvent(source);
			onComplete?.Invoke(true);
			return;
		}
		if (CanShowAd()) {
			adsData[(int)type].source = source;
			adsData[(int)type].onCompleteShow = onComplete;
			Advertisement.Show(adsData[(int)type].UnitId, this);
		} else {
			onComplete?.Invoke(false);
		}
	}
	
	public void OnUnityAdsAdLoaded(string adUnitId) {
		status = Status.AdLoaded;
	}
	
	public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message) {
		Debug.LogError($"Error loading Ad Unit: {adUnitId} - {error.ToString()} - {message}");
		this.Wait(1f, () => LoadAd(GetAdType(adUnitId)));
	}
	
	public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message) {
		Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
		AdType type = GetAdType(adUnitId);
		adsData[(int)type].onCompleteShow?.Invoke(false);
		status = Status.InitializedSuccess;
		this.Wait(1f, () => LoadAd(type));
	}
	
	public void OnUnityAdsShowStart(string adUnitId) { }
	public void OnUnityAdsShowClick(string adUnitId) { }

	public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState) {
		AdType type = GetAdType(adUnitId);
		adsData[(int)type].onCompleteShow?.Invoke(showCompletionState == UnityAdsShowCompletionState.COMPLETED);
		status = Status.InitializedSuccess;
		AnalyticsSystem.RecordWatchAdCompleteEvent(adsData[(int)type].source);
		this.Wait(1f, () => LoadAd(type));
	}

	private static AdType GetAdType(string type) {
		return Enum.Parse<AdType>(type);
	}
	
	private enum Status {
		None,
		InitializedSuccess,
		InitializedFail,
		AdLoaded
	}

	public enum AdType {
		Interstitial_Android,
		Rewarded_Android
	}

	private class AdData {
		public AdType type;
		public string UnitId => type.ToString();
		public Action<bool> onCompleteShow;
		public string source;
	}
}