using UnityEngine;

public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour {

	private static T instance;

	public static T Instance {
		get {
			if (instance == null) {
				instance = FindFirstObjectByType<T>();
				if (instance == null) {
					instance = new GameObject(typeof(T).ToString()).AddComponent<T>();
				}
			}
			return instance;
		}
	}
	
	public static bool HasInstance() {
		return instance != null;
	}

	protected virtual void Awake() {
		instance = this as T;
	}

	private void OnDestroy() {
		instance = null;
	}
}