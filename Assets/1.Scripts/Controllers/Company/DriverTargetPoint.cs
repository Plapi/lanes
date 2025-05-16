using UnityEngine;

public class DriverTargetPoint : MonoBehaviour {

	[SerializeField] private bool available = true;
	
	[Space]
	[SerializeField] private DriversBubbleText bubbleText;
	[SerializeField] private Room room;
	[SerializeField] private int minRoomLevel;
	[SerializeField] private int maxRoomLevel;
	
	public bool IsAvailable {
		get => available && room.RoomData.level >= minRoomLevel && room.RoomData.level <= maxRoomLevel;
		set => available = value;
	}

	public string GetBubbleText() {
		return bubbleText.GetText();
	}
	
}
