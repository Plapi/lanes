using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using DG.Tweening;

public class UserCar : Car {
	
	public int RoadLaneIndex { get; private set; }
	public Segment CurrentSegment { get; private set; }
	
	public RoadLane CurrentRoadLane => CurrentSegment.RoadLanes[RoadLaneIndex];
	
	public void SetSegment(Segment segment, int laneIndex, bool setPosX = true) {
		if (CurrentSegment != null) {
			int diff = segment.BackRoadLanes.Count - CurrentSegment.BackRoadLanes.Count;
			laneIndex += diff;
			laneIndex = Mathf.Clamp(laneIndex, 0, segment.RoadLanes.Count - 1);
		} else {
			if (setPosX) {
				transform.SetX(segment.RoadLanes[laneIndex].transform.position.x + Settings.Instance.laneSize / 2f);	
			}
		}
		RoadLaneIndex = laneIndex;
		CurrentSegment = segment;
	}
	
	public void UpdateCar(float verticalInput) {
		targetPos = new Vector3(CurrentRoadLane.transform.position.x + Settings.Instance.laneSize / 2f, FrontPos.y, transform.position.z + 8f);
		UpdateCarInputs(verticalInput);
	}

	private void UpdateCarInputs(float verticalInput) {
		float targetSpeed = verticalInput * avc.MaxSpeed;
		float accelerationInput = (targetSpeed - avc.CurrentSpeed) / avc.MaxSpeed;	
		float brakeInput = (avc.CurrentSpeed - targetSpeed) / avc.MaxSpeed;
		avc.ProvideInputs(GetSteering(), Mathf.Clamp(accelerationInput, 0f, 0.8f), Mathf.Clamp(brakeInput, 0f, 1f));
	}
	
	public void TrySwitchLane(int add) {
		int newLaneIndex = RoadLaneIndex + add;
		if (newLaneIndex >= 0 && newLaneIndex < CurrentSegment.RoadLanes.Count) {
			RoadLaneIndex = newLaneIndex;
		}
	}

	public float GetCurrentSegmentProgress() {
		return (transform.position.z - CurrentSegment.transform.position.z) / CurrentSegment.Length;
	}

	private readonly List<Vector3> startPoints = new();
	public void GoToStart(Action onComplete) {
		Vector3 dir = transform.forward.normalized;
		Vector3 point0 = new Vector3(CurrentRoadLane.transform.position.x + Settings.Instance.laneSize / 2f, FrontPos.y, CurrentRoadLane.EndPos.z);
		Vector3 point1 = new Vector3(transform.position.x, FrontPos.y, transform.position.z);
		Vector3 vectorToPoint1 = point1 - point0;
		Vector3 projection = Vector3.Dot(vectorToPoint1, dir) * dir;
		Vector3 perpendicularVector = vectorToPoint1 - projection;
		Vector3 perpendicularPoint = point0 + perpendicularVector;
		
		(point0, point1) = (point1, point0);
		point0 = perpendicularPoint + (point0 - perpendicularPoint).normalized * 7.5f;
		point1 = perpendicularPoint + (point1 - perpendicularPoint).normalized * 7.5f;

		startPoints.AddRange(Chaikin.SmoothPath(new List<Vector3> { point0, perpendicularPoint, point1 }, 3));
		
		CinemachineFollow cinemachineFollow = GetComponentInChildren<CinemachineFollow>();
		Vector3 prevOffset = cinemachineFollow.FollowOffset;
		cinemachineFollow.FollowOffset = new Vector3(0f, 1.6f, -5f);
		
		StartCoroutine(TransitStartPoints(() => {
			float value = 0f;
			Vector3 startOffset = cinemachineFollow.FollowOffset;
			DOTween.To(() => value, x => value = x, 1f, 3f)
				.SetEase(Ease.Linear)
				.SetDelay(0.5f)
				.OnUpdate(() => {
					cinemachineFollow.FollowOffset = Vector3.Lerp(startOffset, prevOffset, value);
				});
		}, onComplete));
	}

	private IEnumerator TransitStartPoints(Action onReachFirstPoint, Action onComplete) {
		
		int index = 0;
		while (index < startPoints.Count) {
			targetPos = new Vector3(startPoints[index].x, FrontPos.y, startPoints[index].z);
			UpdateCarInputs(0.5f);
			while (Vector3.Distance(targetPos, transform.position) >= 3f) {
				yield return null;
			}
			index++;
			if (index == 1) {
				onReachFirstPoint();
			}
		}
		onComplete();
	}
	
	protected override void OnDrawGizmos() {
		base.OnDrawGizmos();
		Gizmos.color = Color.red;
		for (int i = 0; i < startPoints.Count; i++) {
			Gizmos.DrawSphere(startPoints[i], 0.25f);
		}
	}
}
