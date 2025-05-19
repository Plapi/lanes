using UnityEngine;
using UnityEngine.UI;

public class UINavigationItem : UIObject {

	[Space]
	[SerializeField] private CanvasGroup canvasGroup;
	
	[Space]
	[SerializeField] private Image arrowDirImage;
	[SerializeField] private Sprite forwardArrowSprite;
	[SerializeField] private Sprite leftArrowSprite;
	[SerializeField] private Sprite rightArrowSprite;
	
	[Space]
	[SerializeField] private Image personImage;

	public void Init(Data data) {
		bool isDir = data is DirData;
		SetActiveItems(data is DirData);
		
		if (isDir) {
			GenerateDir dir = (GenerateDir)data.index;
			arrowDirImage.sprite = dir == GenerateDir.Forward ? forwardArrowSprite : dir == GenerateDir.Left ? leftArrowSprite : rightArrowSprite;
		} else {
			personImage.sprite = RideController.GetPersonSprite(((PersonData)data).group, data.index);
			PersonData personData = (PersonData)data;
			UpdateCurrentPersonState(personData.state);
		}
	}

	public void SetAlpha(float alpha) {
		canvasGroup.alpha = alpha;
	}

	private void SetActiveItems(bool isDir) {
		arrowDirImage.gameObject.SetActive(isDir);
		personImage.gameObject.SetActive(!isDir);
	}

	public void UpdateCurrentPersonState(PersonData.State state) {
		personImage.transform.HideAllChildrenExcept((int)state);
	}

	public abstract class Data {
		public int index;
	}

	public class DirData : Data { }
	
	public class PersonData : Data {
		
		public int group;
		public State state;
		
		public enum State {
			PickUp,
			DropOff,
			Checkmark,
			Missed
		}
	}
}
