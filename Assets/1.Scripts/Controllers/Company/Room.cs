using UnityEngine;

public class Room : MonoBehaviour {

	[SerializeField] private string id;
	[SerializeField] private Element obj;

	private int currentLevel;

	public virtual void Init(RoomData roomData) {
		if (roomData.level != currentLevel) {
			Destroy(obj.gameObject);
			obj = Instantiate(Resources.Load<Element>($"Company/{id}/${id}"), transform);
			currentLevel = roomData.level;
		}
	}
}

public class RoomData {
	public int level;
}
