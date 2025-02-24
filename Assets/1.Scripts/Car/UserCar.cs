using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using DG.Tweening;

public class UserCar : Car {

	public Action OnRequireNewSegments;
	
	public Segment CurrentSegment { get; private set; }
	private Segment nextSegment;
	
	public void SetSegments(Segment currentSegment, Segment nextSegment) {
		CurrentSegment = currentSegment;
		this.nextSegment = nextSegment;
	}
	
	public void UpdateCar(float verticalInput, float horizontalInput) {
		SetTargetPos(horizontalInput);
		UpdateCarInputs(verticalInput);
		if (GetSegmentProgress(nextSegment) > 0.5f) {
			OnRequireNewSegments();
		}
	}

	private void SetTargetPos(float horizontalInput) {
		float progress = Mathf.InverseLerp(CurrentSegment.transform.position.z + CurrentSegment.Length, nextSegment.transform.position.z, transform.position.z);
		float minX = Mathf.Lerp(CurrentSegment.RoadLanes[0].transform.position.x + Settings.Instance.laneSize / 2f - 1f, 
			nextSegment.RoadLanes[0].transform.position.x + Settings.Instance.laneSize / 2f - 1f, 
			progress);
		float maxX = Mathf.Lerp(CurrentSegment.RoadLanes[^1].transform.position.x + Settings.Instance.laneSize / 2f + 1f, 
			nextSegment.RoadLanes[^1].transform.position.x + Settings.Instance.laneSize / 2f + 1f, 
			progress);
		targetPos.x = Mathf.Clamp( Mathf.Lerp(transform.position.x - 4f, transform.position.x + 4f, horizontalInput), minX, maxX);
		targetPos.y = FrontPos.y;
		targetPos.z = transform.position.z + 5f;
	}
	
	private void UpdateCarInputs(float verticalInput) {
		float accelerationInput = Mathf.Lerp(0.1f, 0.5f, verticalInput);
		float brakeInput = Mathf.Lerp(0.5f, 0f, Mathf.InverseLerp(0f, 0.1f, verticalInput));
		avc.ProvideInputs(GetSteering(), accelerationInput, brakeInput);
	}

	private float GetSegmentProgress(Segment segment) {
		return (transform.position.z - segment.transform.position.z) / segment.Length;
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
		point0 = perpendicularPoint + (point0 - perpendicularPoint).normalized * 10f;
		point1 = perpendicularPoint + (point1 - perpendicularPoint).normalized * 10f;

		startPoints.Clear();
		startPoints.AddRange(Chaikin.SmoothPath(new List<Vector3> { point0, perpendicularPoint, point1 }, 3));
		startPoints.RemoveAt(0);
		startPoints.RemoveAt(startPoints.Count - 1);
	}
	
	public void GoToStart(Action onComplete) {
		
		CinemachineFollow cinemachineFollow = GetComponentInChildren<CinemachineFollow>();
		Vector3 prevOffset = cinemachineFollow.FollowOffset;
		cinemachineFollow.FollowOffset = new Vector3(0f, 1.6f, -5f);
		
		StartCoroutine(TransitStartPoints(() => {
			float value = 0f;
			Vector3 startOffset = cinemachineFollow.FollowOffset;
			DOTween.To(() => value, x => value = x, 1f, 2f)
				.SetEase(Ease.Linear)
				.SetDelay(1f)
				.OnUpdate(() => {
					cinemachineFollow.FollowOffset = Vector3.Lerp(startOffset, prevOffset, value);
				});
		}, onComplete));
	}

	private IEnumerator TransitStartPoints(Action onReachFirstPoint, Action onComplete) {
		int index = 0;
		while (index < startPoints.Count) {
			targetPos = new Vector3(startPoints[index].x, FrontPos.y, startPoints[index].z);
			while (Vector3.Distance(targetPos, FrontPos) >= 1f) {
				UpdateCarInputs(0.5f);
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
