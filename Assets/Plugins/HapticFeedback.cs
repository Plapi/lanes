using System.Runtime.InteropServices;
using UnityEngine;

public static class HapticFeedback {

	private static bool hapticEnabled;

	public static void SetEnabled(bool enabled) {
		hapticEnabled = enabled;
	}

	public static void VibrateHaptic(Type type) {
		if (!hapticEnabled) {
			return;
		}
#if UNITY_IOS && !UNITY_EDITOR
        VibrateHaptic((int)type);
#elif UNITY_ANDROID && !UNITY_EDITOR
        AndroidVibrate((int)type);
#endif
	}

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void VibrateHaptic(int type);
#endif

#if UNITY_ANDROID
	private static AndroidJavaObject vibrator;
	private static AndroidJavaObject Vibrator {
		get {
			if (vibrator == null) {
				using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
				vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
			}
			return vibrator;
		}
	}
	
	private static void AndroidVibrate(int type) {
		if (Vibrator == null) return;

		if (AndroidVersion >= 26) {
			var vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
			int effectType = type == 0 ? 1 : // EFFECT_HEAVY_CLICK
				type == 1 ? 0 : // EFFECT_CLICK
				type == 2 ? 5 : // EFFECT_DOUBLE_CLICK
				type == 3 ? 4 : // EFFECT_TICK
				2;             // EFFECT_THUD (alternative for Soft)
                
			var effect = vibrationEffectClass.CallStatic<AndroidJavaObject>("createPredefined", effectType);
			Vibrator.Call("vibrate", effect);
		} else {
			Vibrator.Call("vibrate", 50);
		}
	}

	private static int AndroidVersion {
		get {
			var version = new AndroidJavaClass("android.os.Build$VERSION");
			return version.GetStatic<int>("SDK_INT");
		}
	}
#endif

	public enum Type {
		Heavy = 0,
		Light = 1,
		Medium = 2,
		Rigid = 3,
		Soft = 4
	}
}