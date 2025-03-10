using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using DG.Tweening;
using SkrilStudio;
using Unity.VisualScripting;

public class UserCar : Car {

	[SerializeField] private CinemachineFollow cinemachineFollow;
	[SerializeField] private int maxHealth;
	[SerializeField] private float healthDamageDivider;
	[SerializeField] private int price;
	[SerializeField] private float coinsMultiplier;
	
	[Space]
	[SerializeField] private MaterialAndColorPreset materialAndColorPreset;
	[SerializeField] private MeshRenderer[] meshRenderers;

	[Space]
	[SerializeField] private RealisticEngineSound realisticEngineSound;
	[SerializeField] private float maxEngineRPM;
	[SerializeField] private AudioClip startSound;
	
	public Action OnRequireNewSegments;
	public Action<float> OnHealthUpdate;
	
	public float CoinsMultiplier => coinsMultiplier;
	
	private float currentHealth;
	public int Price => price;
	public float MaxSpeed => avc.MaxSpeed;
	public int MaxHealth => maxHealth;
	
	public Segment CurrentSegment { get; private set; }
	private Segment nextSegment;
	
	public MaterialAndColorPreset MaterialAndColorPreset => materialAndColorPreset;
	
	private void Start() {
		currentHealth = maxHealth;
		realisticEngineSound.carMaxSpeed = MaxSpeed;
	}
	
	public override void DisableCar() {
		base.DisableCar();
		cinemachineFollow.gameObject.SetActive(false);
	}

	public void ResetCar() {
		currentHealth = maxHealth;
		DisableCar();
	}

	public override void SetSoundEnabled(bool enabled) {
		base.SetSoundEnabled(enabled);
		realisticEngineSound.gameObject.SetActive(enabled);
		if (!enabled) {
			realisticEngineSound.gasPedalPressing = false;
			realisticEngineSound.engineCurrentRPM = 0f;
		}
	}

	public void SetAudioVolume(float volume) {
		realisticEngineSound.masterVolume = volume;
		avc.SkidSound.volume = volume;
	}

	public void SetSegments(Segment currentSegment, Segment nextSegment) {
		CurrentSegment = currentSegment;
		this.nextSegment = nextSegment;
	}
	
	public void UpdateCar(float verticalInput, float horizontalInput) {
		SetTargetPos(horizontalInput);
		UpdateCarInputs(verticalInput);
		if (nextSegment != null && GetSegmentProgress(nextSegment) > 0.5f) {
			OnRequireNewSegments();
		}
	}

	private void SetTargetPos(float horizontalInput) {
		if (CurrentSegment == null) {
			targetPos.x = Mathf.Lerp(transform.position.x - 4f, transform.position.x + 4f, horizontalInput);
			targetPos.y = FrontPos.y;
			targetPos.z = FrontPos.z + 2.5f;
			return;
		}
		float progress = Mathf.InverseLerp(CurrentSegment.transform.position.z + CurrentSegment.Length, nextSegment.transform.position.z, transform.position.z);
		float minX = Mathf.Lerp(CurrentSegment.RoadLanes[0].transform.position.x + Settings.Instance.laneSize / 2f - 1f, 
			nextSegment.RoadLanes[0].transform.position.x + Settings.Instance.laneSize / 2f - 1f, 
			progress);
		float maxX = Mathf.Lerp(CurrentSegment.RoadLanes[^1].transform.position.x + Settings.Instance.laneSize / 2f + 1f, 
			nextSegment.RoadLanes[^1].transform.position.x + Settings.Instance.laneSize / 2f + 1f, 
			progress);
		targetPos.x = Mathf.Clamp(Mathf.Lerp(transform.position.x - 4f, transform.position.x + 4f, horizontalInput), minX, maxX);
		targetPos.y = FrontPos.y;
		targetPos.z = FrontPos.z + 2.5f;
	}
	
	private void UpdateCarInputs(float verticalInput) {
		
		float desiredSpeed = verticalInput * avc.MaxSpeed;
		float speedDifference = desiredSpeed - avc.CurrentSpeed;

		float accelerationInput = 0f;
		float brakeInput = 0f;

		if (verticalInput > 0.9f) {
			accelerationInput = 0.5f;
			brakeInput = 0f;
		} else if (verticalInput < 0.1f) {
			accelerationInput = 0f;
			brakeInput = 1f;
		} else if (Mathf.Abs(speedDifference) > 0.1f) {
			if (speedDifference > 0) {
				accelerationInput = Mathf.Clamp(speedDifference / avc.MaxSpeed, 0, 1f);
			} else {
				brakeInput = Mathf.Clamp(-speedDifference / avc.MaxSpeed, 0, 1f);
			}
		}
		
		avc.ProvideInputs(GetSteering(), accelerationInput, brakeInput);
		
		if (realisticEngineSound.gameObject.activeSelf) {
			realisticEngineSound.gasPedalPressing = verticalInput > 0.1f;
			realisticEngineSound.engineCurrentRPM = Mathf.Lerp(0, maxEngineRPM, avc.CurrentSpeed / realisticEngineSound.carMaxSpeed);
		}
	}

	private float GetSegmentProgress(Segment segment) {
		return (transform.position.z - segment.transform.position.z) / segment.Length;
	}

	private AICar lastHitAICar;
	private void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.TryGetComponent(out AICar aiCar) && lastHitAICar != aiCar) {
			float magnitude = collision.relativeVelocity.magnitude;
			if (magnitude < 5f) {
				return;
			}
			lastHitAICar = aiCar;
			currentHealth -= magnitude / healthDamageDivider;
			currentHealth = Mathf.Max(0, currentHealth);
			OnHealthUpdate?.Invoke(currentHealth / maxHealth);
		}
	}

	private readonly List<Vector3> startPoints = new();
	public void SetStartPoints() {
		Vector3 dir = transform.forward.normalized;
		RoadLane roadLane = CurrentSegment.RoadLanes[2];
		Vector3 point0 = new Vector3(roadLane.transform.position.x + Settings.Instance.laneSize / 2f, FrontPos.y, roadLane.EndPos.z);
		Vector3 point1 = new Vector3(transform.position.x, FrontPos.y, transform.position.z);
		Vector3 vectorToPoint1 = point1 - point0;
		Vector3 projection = Vector3.Dot(vectorToPoint1, dir) * dir;
		Vector3 perpendicularVector = vectorToPoint1 - projection;
		Vector3 perpendicularPoint = point0 + perpendicularVector;
		
		(point0, point1) = (point1, point0);
		
		point0 = perpendicularPoint + (point0 - perpendicularPoint).normalized * 12f;
		point1 = perpendicularPoint + (point1 - perpendicularPoint).normalized * 12f;

		startPoints.Add(point0);
		startPoints.Add(perpendicularPoint);
		startPoints.Add(point1);
	}
	
	public void GoToStart(Camera cam, Action onComplete) {
		
		realisticEngineSound.gameObject.SetActive(true);
		realisticEngineSound.transform.parent = cam.transform;
		realisticEngineSound.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		realisticEngineSound.gasPedalPressing = false;
		realisticEngineSound.engineCurrentRPM = 0f;
		
		AudioSource startAudioSource = AudioSystem.Play(startSound, MixerType.CarEngine, () => {
			float prevMaxSpeed = avc.MaxSpeed;
			avc.MaxSpeed = 60;
			EnableCar();
			StartCoroutine(TransitStartPoints(() => {
				avc.MaxSpeed = prevMaxSpeed;
				onComplete();
			}));
			
			Vector3 prevOffset = cinemachineFollow.FollowOffset;
			cinemachineFollow.FollowOffset = new Vector3(0f, 1.6f, -5f);
			CameraTransition(cam, () => {
				float value = 0f;
				Vector3 startOffset = cinemachineFollow.FollowOffset;
				DOTween.To(() => value, x => value = x, 1f, 2f)
					.SetEase(Ease.OutCubic)
					.SetDelay(1f)
					.OnUpdate(() => {
						cinemachineFollow.FollowOffset = Vector3.Lerp(startOffset, prevOffset, value);
					});
			});
		});
		startAudioSource.transform.position = cam.transform.position;
		startAudioSource.volume = realisticEngineSound.masterVolume;
	}

	private void CameraTransition(Camera cam, Action onComplete) {
		
		Vector3 camFromPos = cam.transform.position;
		Quaternion camFromRot = cam.transform.rotation;
		CinemachineBrain cinemachineBrain = cam.GetComponent<CinemachineBrain>();

		cam.transform.position = camFromPos;
		cam.transform.rotation = camFromRot;
		
		float value = 0f;
		DOTween.To(() => value, x => value = x, 1f, 1.5f)
			.SetEase(Ease.OutCubic)
			.OnUpdate(() => {
				
				cinemachineFollow.gameObject.SetActive(true);
				cinemachineBrain.ManualUpdate();
				Vector3 camToPos = cam.transform.position;
				Quaternion camToRot = cam.transform.rotation;
				cinemachineFollow.gameObject.SetActive(false);
				
				cam.transform.position = Vector3.Lerp(camFromPos, camToPos, value);
				cam.transform.rotation = Quaternion.Lerp(camFromRot, camToRot, value);
			}).OnComplete(() => {
				cinemachineFollow.gameObject.SetActive(true);
				onComplete();
			});
	}

	private IEnumerator TransitStartPoints(Action onComplete) {
		for (float p = 0f; p <= 1f; p += 0.001f) {
			Vector3 point = Bezier.GetPoint(startPoints[0], startPoints[1], startPoints[2], p);
			targetPos = new Vector3(point.x, FrontPos.y, point.z);
			while (Vector3.Distance(targetPos, FrontPos) >= 1f) {
				targetPos.y = FrontPos.y;
				UpdateCarInputs(0.9f);
				yield return null;
			}
		}
		float time = 1f;
		while (time > 0f) {
			targetPos.z = FrontPos.z + 1f;
			UpdateCarInputs(0.9f);
			yield return null;
			time -= Time.deltaTime;
		}
		onComplete();
	}

	public void ApplyMaterial(int selection) {
		if (materialAndColorPreset == null) {
			return;
		}
		MaterialAndColor materialAndColor = materialAndColorPreset.items[Mathf.Clamp(selection, 0, materialAndColorPreset.items.Length - 1)];
		for (int i = 0; i < meshRenderers.Length; i++) {
			meshRenderers[i].material = materialAndColor.material;
		}
	}
	
	protected override void OnDrawGizmos() {
		base.OnDrawGizmos();
		if (startPoints.Count > 0) {
			Gizmos.color = Color.red;
			for (float p = 0f; p < 1f; p += 0.001f) {
				Vector3 point0 = Bezier.GetPoint(startPoints[0], startPoints[1], startPoints[2], p);
				Vector3 point1 = Bezier.GetPoint(startPoints[0], startPoints[1], startPoints[2], p + 0.001f);
				Gizmos.DrawLine(point0, point1);
			}
		}
	}
}
