using Unity.Services.Analytics;

public static class AnalyticsSystem {

	public static void RecordCompanyTutorialEvent(int stepId) {
		RecordEvent(new CompanyTutorialEvent("CompanyTutorial", stepId));
	}

	public static void RecordRoomUpgradeEvent(string roomName, int roomLevel) {
		RecordEvent(new RoomUpgradeEvent("RoomUpgrade", roomName, roomLevel));
	}

	public static void RecordBuyTaxiEvent(int slotIndex) {
		RecordEvent(new BuyTaxiEvent("BuyTaxi", slotIndex));
	}

	public static void RecordHireDriverEvent(string driverId) {
		RecordEvent(new HireDriverEvent("HireDriver", driverId));
	}

	public static void RecordFireDriverEvent(string driverId) {
		RecordEvent(new FireDriverEvent("FireDriver", driverId));
	}

	public static void RecordAssignDriverEvent(string driverId, int parkingIndex) {
		RecordEvent(new AssignDriverEvent("AssignDriver", driverId, parkingIndex));
	}

	public static void RecordUpgradeFloorStartEvent(int floorIndex) {
		RecordEvent(new UpgradeFloorStartEvent("UpgradeFloorStart", floorIndex));
	}

	public static void RecordUpgradeFloorCompleteEvent(int floorIndex) {
		RecordEvent(new UpgradeFloorCompleteEvent("UpgradeFloorComplete", floorIndex));
	}

	public static void RecordWatchAdStartEvent(string adSource) {
		RecordEvent(new WatchAdStartEvent("WatchAddStart", adSource));
	}

	public static void RecordWatchAdCompleteEvent(string adSource) {
		RecordEvent(new WatchAdCompleteEvent("WatchAddComplete", adSource));
	}

	public static void RecordBuyProductStartEvent(string productId, string value) {
		RecordEvent(new BuyProductStartEvent("BuyProduct", productId, value));
	}
	
	public static void RecordBuyProductCompleteEvent(string productId, string value) {
		RecordEvent(new BuyProductCompleteEvent("BuyProduct", productId, value));
	}

	public static void RecordDriveTutorialEvent(int stepId) {
		RecordEvent(new DriveTutorialEvent("DriveTutorialEvent", stepId));
	}

	public static void RecordBuyCarEvent(int carId) {
		RecordEvent(new BuyCarEvent("BuyCar", carId));
	}

	public static void RecordDriveEndlessStartEvent(int carId) {
		RecordEvent(new DriveEndlessStartEvent("DriveEndlessStartEvent", carId));
	}

	public static void RecordDriveEndlessEndEvent(string carId, int distance, int persons, int coins) {
		RecordEvent(new DriveEndlessEndEvent("DriveEndlessEndEvent", carId, distance, persons, coins));
	}

	public static void RecordDriveMissionStartEvent(string carId, int coins) {
		RecordEvent(new DriveMissionStartEvent("DriveMissionStart", carId, coins));
	}

	public static void RecordDriveMissionEndEvent(string carId, int coins, int stars) {
		RecordEvent(new DriveMissionEndEvent("DriveMissionEnd", carId, coins, stars));
	}

	public static void RecordSettingsEvent(int soundFxLevel, int musicLevel, int carEngineLevel, bool haptic) {
		RecordEvent(new SettingsEvent("Settings", soundFxLevel, musicLevel, carEngineLevel, haptic));
	}

	public static void RecordOpenAboutEvent() {
		RecordEvent("OpenAbout");
	}

	public static void RecordClickMailEvent() {
		RecordEvent("ClickMail");
	}

	// Internals

	private static void RecordEvent(Event ev) {
		if (Settings.Instance.enableAnalytics) {
			AnalyticsService.Instance.RecordEvent(ev);	
		}
	}

	private static void RecordEvent(string eventName) {
		if (Settings.Instance.enableAnalytics) {
			AnalyticsService.Instance.RecordEvent(eventName);	
		}
	}

	// Event classes

	private class CompanyTutorialEvent : Event {
		public CompanyTutorialEvent(string name, int stepId) : base(name) {
			SetParameter("stepId", stepId);
		}
	}

	private class RoomUpgradeEvent : Event {
		public RoomUpgradeEvent(string name, string roomName, int roomLevel) : base(name) {
			SetParameter("roomName", roomName);
			SetParameter("roomLevel", roomLevel);
		}
	}

	private class BuyTaxiEvent : Event {
		public BuyTaxiEvent(string name, int slotIndex) : base(name) {
			SetParameter("slotIndex", slotIndex);
		}
	}

	private class HireDriverEvent : Event {
		public HireDriverEvent(string name, string driverId) : base(name) {
			SetParameter("driverId", driverId);
		}
	}

	private class FireDriverEvent : Event {
		public FireDriverEvent(string name, string driverId) : base(name) {
			SetParameter("driverId", driverId);
		}
	}

	private class AssignDriverEvent : Event {
		public AssignDriverEvent(string name, string driverId, int parkingIndex) : base(name) {
			SetParameter("driverId", driverId);
			SetParameter("parkingIndex", parkingIndex);
		}
	}

	private class UpgradeFloorStartEvent : Event {
		public UpgradeFloorStartEvent(string name, int floorIndex) : base(name) {
			SetParameter("floorIndex", floorIndex);
		}
	}

	private class UpgradeFloorCompleteEvent : Event {
		public UpgradeFloorCompleteEvent(string name, int floorIndex) : base(name) {
			SetParameter("floorIndex", floorIndex);
		}
	}

	private class WatchAdStartEvent : Event {
		public WatchAdStartEvent(string name, string adSource) : base(name) {
			SetParameter("addSource", adSource);
		}
	}

	private class WatchAdCompleteEvent : Event {
		public WatchAdCompleteEvent(string name, string adSource) : base(name) {
			SetParameter("addSource", adSource);
		}
	}
	
	private class BuyProductStartEvent : Event {
		public BuyProductStartEvent(string name, string productId, string value) : base(name) {
			SetParameter("productId", productId);
			SetParameter("value", value);
		}
	}
	
	private class BuyProductCompleteEvent : Event {
		public BuyProductCompleteEvent(string name, string productId, string value) : base(name) {
			SetParameter("productId", productId);
			SetParameter("value", value);
		}
	}

	private class DriveTutorialEvent : Event {
		public DriveTutorialEvent(string name, int stepId) : base(name) {
			SetParameter("stepId", stepId);
		}
	}

	private class BuyCarEvent : Event {
		public BuyCarEvent(string name, int carId) : base(name) {
			SetParameter("carId", carId);
		}
	}

	private class DriveEndlessStartEvent : Event {
		public DriveEndlessStartEvent(string name, int carId) : base(name) {
			SetParameter("carId", carId);
		}
	}

	private class DriveEndlessEndEvent : Event {
		public DriveEndlessEndEvent(string name, string carId, int distance, int persons, int coins) : base(name) {
			SetParameter("carId", carId);
			SetParameter("distance", distance);
			SetParameter("persons", persons);
			SetParameter("coins", coins);
		}
	}

	private class DriveMissionStartEvent : Event {
		public DriveMissionStartEvent(string name, string carId, int coins) : base(name) {
			SetParameter("carId", carId);
			SetParameter("coins", coins);
		}
	}

	private class DriveMissionEndEvent : Event {
		public DriveMissionEndEvent(string name, string carId, int coins, int stars) : base(name) {
			SetParameter("carId", carId);
			SetParameter("coins", coins);
			SetParameter("stars", stars);
		}
	}

	private class SettingsEvent : Event {
		public SettingsEvent(string name, int soundFxLevel, int musicLevel, int carEngineLevel, bool haptic) : base(name) {
			SetParameter("soundFxLevel", soundFxLevel);
			SetParameter("musicLevel", musicLevel);
			SetParameter("carEngineLevel", carEngineLevel);
			SetParameter("haptic", haptic);
		}
	}
}
