using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour {

	[SerializeField] private Camera mainCamera;
	[SerializeField] private InputManager inputManager;
	[SerializeField] private UserCar userCar;
	[SerializeField] private AudioClip successClip;
	
	private readonly List<Segment> segments = new(4);
	private Segment currentSegment;
	private Segment leftSegment;
	private Segment rightSegment;
	private Segment nextSegment;
	private Intersection intersection;

	private UITutorialPanel tutorialPanel;

	private readonly string[] tutorialTexts = {
		"Hi, welcome to <color=#FFFF00>Quick Lane Driver!</color>\nBefore you hit the road, let's go through a <color=#FFFF00>quick tutorial</color> to learn how to drive.",
		"To drive your car, <color=#FFFF00>touch and hold</color> the bottom part of the screen. Move your finger <color=#FFFF00>UP</color> to accelerate and <color=#FFFF00>DOWN</color> to slow down. Try it now!",
		"Great! Now let’s learn how to <color=#FFFF00>steer</color>. Move your finger <color=#FFFF00>LEFT</color> to turn left and <color=#FFFF00>RIGHT</color> to turn right. Try it out!",
		"You’ve <color=#FFFF00>mastered</color> the basics! Now hit the road, pick up passengers, and become the <color=#FFFF00>best driver</color> in town!"
	};
	
	private void Start() {
		DontDestroyOnLoad(ObjectPoolManager.Instance);
		AudioSystem.Init(this, PlayerPrefsManager.UserData.volumes);
		
		inputManager.UpdateVerticalInput(0.4f);
		inputManager.enabled = false;
		
		InitFirstSegments();
		
		userCar.ApplyMaterial(PlayerPrefsManager.UserData.carColors[4]);
		userCar.SetAudioVolume(PlayerPrefsManager.UserData.volumes[(int)MixerType.CarEngine]);
		userCar.SetSegments(currentSegment, nextSegment);
		userCar.SetStartPoints();
		userCar.SetSoundEnabled(true);
		userCar.SetEngineSoundToCamera(mainCamera);
		userCar.OnRequireNewSegments = () => {
			InitNextSegments();
			userCar.SetSegments(currentSegment, nextSegment);
		};

		UIController.Instance.Init();
		tutorialPanel = UIController.Instance.GetPanel<UITutorialPanel>();
		tutorialPanel.Init(new UITutorialPanel.Data());

		StartCoroutine(Tutorial());
	}

	private IEnumerator Tutorial() {

		AnalyticsService.Instance.RecordEvent(new TutorialEvent("Tutorial", 0));
		
		yield return new WaitForSeconds(2f);
		
		tutorialPanel.ShowText(tutorialTexts[0]);
		
		yield return new WaitForSeconds(3f);

		bool advance = false;
		tutorialPanel.ShowNextButton(() => {
			advance = true;
		});

		yield return new WaitUntil(() => advance);
		AnalyticsService.Instance.RecordEvent(new TutorialEvent("Tutorial", 1));
		
		tutorialPanel.HideNextButton();
		tutorialPanel.ShowText(tutorialTexts[1]);
		
		yield return new WaitForSeconds(2f);
		
		inputManager.enabled = true;
		tutorialPanel.ShowVerticalAnim();

		yield return new WaitUntil(() => inputManager.VerticalInput < 0.2f);
		yield return new WaitUntil(() => inputManager.VerticalInput > 0.8f);
		
		AnalyticsService.Instance.RecordEvent(new TutorialEvent("Tutorial", 2));

		AudioSystem.Play(successClip);
		tutorialPanel.HideText();
		tutorialPanel.HideVerticalAnim();
		yield return new WaitForSeconds(1f);
		
		tutorialPanel.ShowText(tutorialTexts[2]);
		
		yield return new WaitForSeconds(1f);
		tutorialPanel.ShowHorizontalAnim();
		
		yield return new WaitUntil(() => inputManager.HorizontalInput < 0.3f);
		yield return new WaitUntil(() => inputManager.HorizontalInput > 0.7f);
		
		AnalyticsService.Instance.RecordEvent(new TutorialEvent("Tutorial", 3));
		
		AudioSystem.Play(successClip);
		tutorialPanel.HideText();
		tutorialPanel.HideHorizontalAnim();
		yield return new WaitForSeconds(1f);
		
		tutorialPanel.ShowText(tutorialTexts[3]);
		yield return new WaitForSeconds(1f);
		advance = false;
		tutorialPanel.ShowNextButton(() => {
			advance = true;
		});
		
		yield return new WaitUntil(() => advance);
		
		AnalyticsService.Instance.RecordEvent(new TutorialEvent("Tutorial", 4));
		
		UIController.Instance.FadeInToBlack(() => {
			PlayerPrefsManager.UserData.isTutorialDone = true;
			PlayerPrefsManager.SaveUserData();
			SceneManager.LoadScene("Game");
		});
	}

	private void Update() {
		userCar.UpdateCar(inputManager.VerticalInput, inputManager.HorizontalInput);
	}
	
	private void InitNextSegments() {
		segments.Clear();
		currentSegment.Clear();
		leftSegment.Clear();
		rightSegment.Clear();
		intersection.Clear();
		currentSegment = nextSegment;
		currentSegment.name = "CurrentSegment";
		currentSegment.ClearNextRoadLanes();
		segments.Add(currentSegment);
		
		CreateNextSegments();
		for (int i = 0; i < segments.Count; i++) {
			segments[i].SetStartAndEndPosForRoadLanes();
		}
		intersection.CreateRoadConnections();
		
		currentSegment.ContinueGenerateEnvIfNeeded(leftSegment, rightSegment);
		nextSegment.CreteTopLeftEnvironment(leftSegment);
		nextSegment.CreteTopRightEnvironment(rightSegment);
	}

	private void InitFirstSegments() {
		currentSegment = NewSegment("CurrentSegment", GameController.GetSegmentData(new SegmentInputData {
			backLanes = 2,
			frontLanes = 2,
			length = 200
		}));
		segments.Add(currentSegment);
		CreateNextSegments();
		for (int i = 0; i < segments.Count; i++) {
			segments[i].SetStartAndEndPosForRoadLanes();
		}
		intersection.CreateRoadConnections();
		currentSegment.CreateBottomLeftEnvironment(leftSegment);
		currentSegment.CreateBottomRightEnvironment(rightSegment);
		nextSegment.CreteTopLeftEnvironment(leftSegment);
		nextSegment.CreteTopRightEnvironment(rightSegment);
	}
	
	private void CreateNextSegments() {
		leftSegment = NewSegment("LeftSegment", GetRandomSegmentData(),-90f);
		rightSegment = NewSegment("RightSegment", GetRandomSegmentData(), -90f);
		nextSegment = NewSegment("NextSegment", GetRandomSegmentData());
		
		segments.Add(leftSegment);
		segments.Add(rightSegment);
		segments.Add(nextSegment);
		
		leftSegment.AlignHorizontalWith(rightSegment);
		nextSegment.AlignVerticalWith(currentSegment);
		
		leftSegment.transform.SetLocalX(Mathf.Min(currentSegment.transform.localPosition.x, nextSegment.transform.localPosition.x));
		rightSegment.transform.SetLocalX(Mathf.Max(currentSegment.transform.localPosition.x + currentSegment.Width, nextSegment.transform.localPosition.x + nextSegment.Width) + rightSegment.Length);
		
		float addZ = currentSegment.transform.localPosition.z + currentSegment.Length - Mathf.Min(leftSegment.transform.localPosition.z, rightSegment.transform.localPosition.z);
		leftSegment.transform.SetLocalZ(leftSegment.transform.localPosition.z + addZ);
		rightSegment.transform.SetLocalZ(rightSegment.transform.localPosition.z + addZ);
		
		nextSegment.transform.SetLocalZ(Mathf.Max(leftSegment.transform.localPosition.z + leftSegment.Width, rightSegment.transform.localPosition.z + rightSegment.Width));
		
		intersection = Instantiate(Resources.Load<Intersection>("Intersection/Intersection"), transform);
		intersection.name = "Intersection";
		intersection.transform.SetLocalX(leftSegment.transform.localPosition.x);
		intersection.transform.SetLocalZ(currentSegment.transform.localPosition.z + currentSegment.Length);
		intersection.Init(currentSegment, leftSegment, rightSegment, nextSegment);
	}
	
	private Segment NewSegment(string segmentName, SegmentData segmentData, float angle = 0f) {
		Segment segment = new GameObject(segmentName).AddComponent<Segment>();
		segment.transform.parent = transform;
		segment.Init(segmentData);
		segment.transform.SetLocalAngleY(angle);
		return segment;
	}
	
	private static SegmentData GetRandomSegmentData() {
		SegmentInputData segmentInputData = new() {
			backLanes = Random.Range(2, 5),
			frontLanes = Random.Range(2, 5),
			length = Settings.Instance.laneSize * Random.Range(40, 100)
		};
		return GameController.GetSegmentData(segmentInputData);
	}
	
	private class TutorialEvent : Unity.Services.Analytics.Event {
		public TutorialEvent(string name, int stepId) : base(name) {
			SetParameter("stepId", stepId);
		}
	}
}