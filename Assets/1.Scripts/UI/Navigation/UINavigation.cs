using UnityEngine;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UINavigation : UIObject {

	[SerializeField] private UINavigationItem currentItem;
	
	private UINavigationItem leftItem;
	private UINavigationItem rightItem;
	private UINavigationItem auxItem;

	private bool alreadyGenerated;

	public void Init(UINavigationItem.Data currentItemData, UINavigationItem.Data nextItemData) {
		if (!alreadyGenerated) {
			GenerateItems();
			alreadyGenerated = true;
		}
		currentItem.Init(currentItemData);
		rightItem.Init(nextItemData);
		SetRight(rightItem);
		SetRight(auxItem);
		leftItem.gameObject.SetActive(false);
		auxItem.gameObject.SetActive(false);
	}

	public void NextItem(UINavigationItem.Data nexItem) {
		bool isFirst = !leftItem.gameObject.activeSelf;
		
		UINavigationItem tempItem = leftItem;
		leftItem = currentItem;
		leftItem.name = "LeftItem";
			
		currentItem = rightItem;
		currentItem.name = "CurrentItem";

		rightItem = auxItem;
		rightItem.name = "RightItem";
		SetRight(rightItem);
		rightItem.Init(nexItem);
		rightItem.SetAlpha(0f);
		rightItem.gameObject.SetActive(true);

		auxItem = tempItem;
		auxItem.name = "AuxItem";
			
		float value = 0f;
		DOTween.To(() => value, x => value = x, 1f, UIController.defaultTime)
			.SetEase(Ease.OutCubic)
			.OnUpdate(() => {
				leftItem.SetAlpha(Mathf.Lerp(1f, 0.2f, value));
				leftItem.RectTransform.SetAnchorPosX(Mathf.Lerp(0, -80f, value));
				leftItem.transform.SetScale(Mathf.Lerp(1f, 0.8f, value));
					
				currentItem.SetAlpha(Mathf.Lerp(0.2f, 1f, value));
				currentItem.RectTransform.SetAnchorPosX(Mathf.Lerp(80, 0f, value));
				currentItem.transform.SetScale(Mathf.Lerp(0.8f, 1f, value));
					
				rightItem.SetAlpha(Mathf.Lerp(0f, 0.2f, value));

				if (!isFirst) {
					auxItem.SetAlpha(Mathf.Lerp(0.2f, 0f, value));
				}
			});
	}

	public void UpdateCurrentPersonState(UINavigationItem.PersonData.State state) {
		currentItem.UpdateCurrentPersonState(state);
	}

	private static void SetRight(UINavigationItem item) {
		item.SetAlpha(0.2f);
		item.RectTransform.SetAnchorPosX(80f);
		item.transform.SetScale(0.8f);
	}

	private void GenerateItems() {
		leftItem = GenerateItem("LeftItem", 0);
		rightItem = GenerateItem("RightItem", 2);
		auxItem = GenerateItem("AuxItem", 3);
	}

	private UINavigationItem GenerateItem(string name, int index) {
		UINavigationItem item = Instantiate(currentItem.gameObject, transform).GetComponent<UINavigationItem>();
		item.name = name;
		item.transform.SetSiblingIndex(index);
		return item;
	}

}

#if UNITY_EDITOR
[CustomEditor(typeof(UINavigation))]
public class UINavigationEditor : Editor {
	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		
		UINavigation nav = (UINavigation)target;
		
		GUILayout.Space(10f);
		if (GUILayout.Button("Init")) {
			nav.Init(new UINavigationItem.DirData { index = (int)GenerateDir.Forward }, 
				new UINavigationItem.PersonData { index = 3, state = UINavigationItem.PersonData.State.PickUp });
		}
		if (GUILayout.Button("Next Item Dir")) {
			nav.NextItem(new UINavigationItem.DirData { index = Random.Range(0, 3) });
		}
		if (GUILayout.Button("Next Item Person")) {
			nav.NextItem(new UINavigationItem.PersonData { index = Random.Range(0, 9), state = UINavigationItem.PersonData.State.PickUp });
		}
	}
}
#endif
