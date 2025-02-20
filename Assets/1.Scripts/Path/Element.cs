using UnityEngine;

public class Element : MonoBehaviour, IPoolableObject<Element> {

	[SerializeField] private string id;
	
	public string Id {
		get => id;
		set => id = value;
	}

	public Element GetMonoBehaviour() {
		return this;
	}
	
	public Element Create(string name, Transform parent, float angleY, float x, float z) {
		Element element = ObjectPoolManager.Get(this, parent);
		element.name = name;
		element.transform.SetAngleY(angleY);
		element.transform.SetXZ(x, z);
		element.gameObject.SetActive(true);
		return element;
	}

	public Element Create(string name, Transform parent, float x, float z) {
		Element element = ObjectPoolManager.Get(this, parent);
		element.name = name;
		element.transform.SetLocalXZ(x, z);
		element.gameObject.SetActive(true);
		return element;
	}
}
