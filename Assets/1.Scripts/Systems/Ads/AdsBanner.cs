using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsBanner : MonoBehaviour {

	[SerializeField] private string androidAdUnitId = "Banner_Android";
	[SerializeField] private string iOSAdUnitId = "Banner_iOS";
	
	private string addUnitId;
	private bool bannerVisible;
	
	public IEnumerator Start() {

		yield return new WaitUntil(() => AdsController.HasInstance() && AdsController.Instance.WasInitSuccessful);
		
		addUnitId = Application.platform == RuntimePlatform.IPhonePlayer ? iOSAdUnitId : androidAdUnitId;
		Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
		Advertisement.Banner.Load(addUnitId, new BannerLoadOptions {
			loadCallback = () => {
				Advertisement.Banner.Show(addUnitId, new BannerOptions {
					hideCallback = () => bannerVisible = false,
					showCallback = () => bannerVisible = true
				});
				bannerVisible = true;
			}, errorCallback = error => {
				Debug.LogError($"Banner Error: {error}");
			}
		});
	}

	private void OnDestroy() {
		if (Application.isPlaying && bannerVisible) {
			Advertisement.Banner.Hide();
		}
	}
}
