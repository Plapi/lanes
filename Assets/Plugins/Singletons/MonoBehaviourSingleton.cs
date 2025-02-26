using UnityEngine;

public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour {

	private static T instance;
	public static T Instance {
		get {
			if (instance == null) {
				instance = new GameObject(typeof(T).ToString()).AddComponent<T>();
			}
			return instance;
		}
	}

	protected virtual void Awake() {
		instance = this as T;
		// DontDestroyOnLoad(gameObject);
	}

	private void OnDestroy() {
		instance = null;
	}
}
