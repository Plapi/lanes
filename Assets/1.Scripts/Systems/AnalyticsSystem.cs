using Unity.Services.Analytics;
using UnityEngine;

public class AnalyticsSystem {

	public static void RecordTutorialEvent(int stepId) {
		AnalyticsService.Instance.RecordEvent(new TutorialEvent("Tutorial", stepId));
	}

	public static void RecordSettingsEvent(int soundFxLevel, int musicLevel, int carEngineLevel, bool haptic) {
		AnalyticsService.Instance.RecordEvent(new SettingsEvent("Settings", soundFxLevel, musicLevel, carEngineLevel, haptic));
	}
	
	public static void RecordRaceStartEvent(int carId) {
		AnalyticsService.Instance.RecordEvent(new RaceStartEvent("RaceStart", carId));
	}

	public static void RecordRaceEndEvent(int carId, int distance, int persons, int coins) {
		AnalyticsService.Instance.RecordEvent(new RaceEndEvent("RaceEnd", carId, distance, persons, coins));
	}

	public static void RecordBuyCarEvent(int carId) {
		AnalyticsService.Instance.RecordEvent(new BuyCarEvent("BuyCar", carId));
	}
	
	private class TutorialEvent : Unity.Services.Analytics.Event {
		public TutorialEvent(string name, int stepId) : base(name) {
			SetParameter("stepId", stepId);
		}
	}
	
	private class SettingsEvent : Unity.Services.Analytics.Event {
		public SettingsEvent(string name, int soundFxLevel, int musicLevel, int carEngineLevel, bool haptic) : base(name) {
			SetParameter("soundFxLevel", soundFxLevel);
			SetParameter("musicLevel", musicLevel);
			SetParameter("carEngineLevel", carEngineLevel);
			SetParameter("haptic", haptic);
		}
	}
	
	private class RaceStartEvent : Unity.Services.Analytics.Event {
		public RaceStartEvent(string name, int carId) : base(name) {
			SetParameter("carId", carId);
		}
	}
	
	private class RaceEndEvent : Unity.Services.Analytics.Event {
		public RaceEndEvent(string name, int carId, int distance, int persons, int coins) : base(name) {
			SetParameter("carId", carId);
			SetParameter("distance", distance);
			SetParameter("persons", persons);
			SetParameter("coins", coins);
		}
	}

	private class BuyCarEvent : Unity.Services.Analytics.Event {
		public BuyCarEvent(string name, int carId) : base(name) {
			SetParameter("carId", carId);
		}
	}

}
