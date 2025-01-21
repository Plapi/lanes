using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviourSingleton<ObjectPoolManager> {

	private readonly Dictionary<string, Queue<MonoBehaviour>> pools = new();

	public static void CreatePool<T>(IPoolableObject<T> poolableObject, int size = 10) where T : MonoBehaviour {
		if (!Application.isPlaying) {
			return;
		}
		Instance.CreatePoolPrivate(poolableObject, size);
	}
	
	private void CreatePoolPrivate<T>(IPoolableObject<T> poolableObject, int size) where T : MonoBehaviour {
		if (pools.ContainsKey(poolableObject.Id)) {
			return;
		}
		Queue<MonoBehaviour> queue = new();
		for (int i = 0; i < size; i++) {
			T obj = poolableObject.GetMonoBehaviour();
			queue.Enqueue(InstantiateObj(poolableObject, transform));
			obj.gameObject.SetActive(false);
		}
		pools.Add(poolableObject.Id, queue);
	}

	public static T Get<T>(IPoolableObject<T> poolableObject, Transform parent = null) where T : MonoBehaviour {
		return Application.isPlaying ?
			Instance.GetPrivate(poolableObject, parent) :
			InstantiateObj(poolableObject, parent);
	}

	private T GetPrivate<T>(IPoolableObject<T> poolableObject, Transform parent) where T : MonoBehaviour {
		if (!pools.TryGetValue(poolableObject.Id, out Queue<MonoBehaviour> queue)) {
			Debug.LogError($"The pool for {poolableObject.Id} does not exist.");
			return null;
		}
		if (queue.Count == 0) {
			queue.Enqueue(InstantiateObj(poolableObject));
		}
		T obj = (T)queue.Dequeue();
		obj.gameObject.SetActive(true);
		obj.transform.parent = parent;
		obj.transform.localPosition = Vector3.zero;
		return obj;
	}
	
	public static void Release<T>(IPoolableObject<T> poolableObject) where T : MonoBehaviour {
		if (Application.isPlaying) {
			Instance.ReleasePrivate(poolableObject);
		} else {
			DestroyImmediate(poolableObject.GetMonoBehaviour().gameObject);
		}
	}

	private void ReleasePrivate<T>(IPoolableObject<T> poolableObject) where T : MonoBehaviour {
		if (!pools.TryGetValue(poolableObject.Id, out Queue<MonoBehaviour> queue)) {
			Debug.LogError($"The pool for {poolableObject.Id} does not exist.");
			return;
		}
		T obj = poolableObject.GetMonoBehaviour();
		obj.transform.parent = transform;
		obj.transform.localPosition = Vector3.zero;
		obj.gameObject.SetActive(false);
		queue.Enqueue(obj);
	}

	private static T InstantiateObj<T>(IPoolableObject<T> poolableObject, Transform parent = null) where T : MonoBehaviour {
		T obj = Instantiate(poolableObject.GetMonoBehaviour(), parent);
		((IPoolableObject<T>)obj).Id = poolableObject.Id;
		obj.transform.localPosition = Vector3.zero;
		return obj;
	}
}

public interface IPoolableObject<out T> where T : MonoBehaviour {
	public string Id { get; set; }
	public T GetMonoBehaviour();
}