using System;
using Unity.Notifications.Android;
using UnityEngine;
using UnityEngine.Android;

public static class NotificationSystem {
	
	private const string notificationTag = "android.permission.POST_NOTIFICATIONS";

	private static bool HasAuthorization() {
		return Permission.HasUserAuthorizedPermission(notificationTag);
	}
	
	public static void RequestPermissionNotification() {
		if (!HasAuthorization()) {
			Permission.RequestUserPermission(notificationTag);
		}
	}
	
	public static void ScheduleVaultStorageNotification(DateTime fireTime) {
		if (!HasAuthorization()) {
			return;
		}
		
		AndroidNotificationChannel channel = new() {
			Id = "default_channel",
			Name = "Default Channel",
			Importance = Importance.Default,
			Description = "Generic notifications",
		};
		AndroidNotificationCenter.RegisterNotificationChannel(channel);
		
		var notification = new AndroidNotification {
			Title = "💰 Vault Storage is Full!",
			Text = "You've made a fortune! Grow your empire now! 🏢",
			LargeIcon = "icon_0",
			FireTime = fireTime
		};
		
		AndroidNotificationCenter.SendNotification(notification, "default_channel");
	}

	public static void CancelAllNotifications() {
		if (!HasAuthorization()) {
			return;
		}
		AndroidNotificationCenter.CancelAllNotifications();
	}
}
