using System;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using DG.Tweening;

public class Driver : Element {
	
	private static readonly int animatorSpeedId = Animator.StringToHash("Speed");

	[SerializeField] private NavMeshAgent agent;
	[SerializeField] private DriverTargetPoint targetPoint;
	
	[Space]
	[SerializeField] private Transform bubblePivot;
	[SerializeField] private TextMeshProUGUI bubbleText;
	[SerializeField] private AudioSource bubbleAudio;
	[SerializeField] private float bubbleAudioDistance;
	
	private Animator animator;
	private Action onTargetReached;
	
	private Transform bubbleLookAt;
	
	private void Start() {
		transform.GetChild(0).gameObject.SetActive(false);
		transform.GetChild(1).gameObject.SetActive(false);
		DriverDesignData[] drivers = Settings.Instance.company.drivers;
		DriverDesignData randomDriver = drivers[UnityEngine.Random.Range(0, drivers.Length)];
		string[] split = randomDriver.characterId.Split('_', 2);
		animator = transform.GetChild(int.Parse(split[0])).GetComponent<Animator>();
		animator.gameObject.SetActive(true);
		HideAllCharacters(animator.transform, split[1]);
	}

	public void Init( Transform bubbleLookAt) {
		this.bubbleLookAt = bubbleLookAt;
		bubblePivot.parent.gameObject.SetActive(false);
		bubblePivot.DOKill();
	}

	public void SetTargetPoint(DriverTargetPoint targetPoint, Action onTargetReached) {
		this.targetPoint = targetPoint;
		this.onTargetReached = onTargetReached;
		agent.destination = targetPoint.transform.position;
	}
	
	private void Update() {
		animator.SetFloat(animatorSpeedId, agent.velocity.magnitude);
		if (agent.velocity.sqrMagnitude > 0.001f) {
			transform.SetAngleY(Quaternion.LookRotation(agent.velocity - Vector3.zero).eulerAngles.y);	
		}
		
		if (targetPoint != null && AgentReachedTarget()) {
			onTargetReached?.Invoke();
			onTargetReached = null;
			targetPoint = null;
		}
	}

	public void ShowBubble(string text) {
		bubbleText.text = text;
		bubblePivot.parent.gameObject.SetActive(true);
		bubblePivot.SetScale(0.01f);
		bubblePivot.DOScale(1f, 0.2f).SetEase(Ease.OutExpo);
		if (Vector3.Distance(transform.position, bubbleLookAt.position) <= bubbleAudioDistance) {
			bubbleAudio.Play();
		}
	}
	
	public void HideBubble() {
		bubblePivot.DOScale(0.01f, 0.2f).SetEase(Ease.InExpo).OnComplete(() => {
			bubblePivot.parent.gameObject.SetActive(false);
		});
	}

	private void LateUpdate() {
		if (bubbleLookAt != null && bubblePivot.gameObject.activeSelf) { 
			Vector3 direction = bubblePivot.position - bubbleLookAt.position;
			bubblePivot.rotation = Quaternion.LookRotation(direction); 
		}
	}

	private bool AgentReachedTarget() {
		return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
	}

	private static void HideAllCharacters(Transform character, string exceptName) {
		for (int i = 0; i < character.childCount - 1; i++) {
			Transform child = character.GetChild(i);
			child.gameObject.SetActive(child.name == exceptName);
		}
	}
}
