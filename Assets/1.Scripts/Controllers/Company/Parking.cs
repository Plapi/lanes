using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parking : MonoBehaviour {

	[SerializeField] private AICarPath[] exitPath;
	[SerializeField] private AICarPath[] enterPath;

	private AICar aiCar;
	private RoadLane roadLane;
	private float initCarSpeed;
	private float initCarPosY;

	public bool HasCar() {
		return aiCar != null;
	}

	public void Init(RoadLane roadLane) {
		this.roadLane = roadLane;
	}

	public void ReleaseCarIfNeeded() {
		if (aiCar != null) {
			ObjectPoolManager.Release(aiCar);
			aiCar = null;
		}
	}

	public void SetCar() {
		if (aiCar != null) {
			Debug.LogError("Ignore setting car");
			return;
		}
		aiCar = CreateCar();
		aiCar.SetTargetPoint(null);
		SetCarPosStart();
		aiCar.gameObject.SetActive(true);
	}

	private void SetCarPosStart() {
		aiCar.gameObject.SetActive(false);
		aiCar.DisableCar();
		aiCar.transform.position = new Vector3(transform.position.x, initCarPosY, transform.position.z);
		aiCar.transform.rotation = transform.rotation;
		aiCar.EnableCar();
		aiCar.gameObject.SetActive(true);
	}

	public void ExitCar(Action onExit) {
		if (aiCar == null) {
			Debug.LogError("Ignore exit");
			return;
		}
		SetCarPosStart();
		aiCar.MaxSpeed = 20f;
		SetExitCarPath(0, () => {
			aiCar.MaxSpeed = initCarSpeed;
			aiCar.SetTargetPoint(new TargetPoint {
				pos = roadLane.EndPos,
				onReach = ObjectPoolManager.Release,
				allowPassing = () => true
			});
			aiCar = null;
			onExit?.Invoke();
		});
	}

	private void SetExitCarPath(int index, Action onComplete) {
		if (index >= exitPath.Length) {
			onComplete();
			return;
		}
		exitPath[index].SetCar(aiCar, () => { SetExitCarPath(index + 1, onComplete); }, () => {
			if (index == exitPath.Length - 1) {
				float progress = exitPath[^1].GetProgress();
				if (progress > 0.4f && progress < 0.5f) {
					Vector3 rayDir = (roadLane.StartPos - roadLane.EndPos).normalized;
					Utils.GetIntersection(aiCar.FrontPos, aiCar.transform.forward,
						roadLane.EndPos, rayDir, out Vector3 intersection);
					Debug.DrawRay(intersection, rayDir * 20f, Color.red);
					return !aiCar.Raycast(intersection, rayDir, 20f, out _);
				}
				return true;
			}
			return true;
		});
	}

	public void EnterCar(Action onComplete) {
		if (aiCar != null) {
			Debug.LogError("Ignore entering car");
			return;
		}
		aiCar = CreateCar();
		aiCar.transform.position = new Vector3(enterPath[0].transform.position.x, initCarPosY, enterPath[0].transform.position.z);
		aiCar.transform.rotation = enterPath[0].transform.rotation;
		StartCoroutine(WaitingForFreePath(() => {
			aiCar.gameObject.SetActive(true);
			SetEnterCarPath(0, () => {
				aiCar.SetTargetPoint(null);
				onComplete();
			});
		}));
	}

	private IEnumerator WaitingForFreePath(Action onComplete) {
		while (!CanSpawnAICarOnRoadLane()) {
			yield return null;
			if (aiCar == null) {
				yield break;
			}
		}
		onComplete?.Invoke();
	}

	private bool CanSpawnAICarOnRoadLane() {
		if (aiCar == null) {
			return false;
		}
		List<AICar> aiCars = roadLane.GetAICars();
		for (int i = 0; i < aiCars.Count; i++) {
			if (Vector3.Distance(aiCars[i].transform.position, aiCar.transform.position) < 20f) {
				return false;
			}
		}
		return true;
	}

	private void SetEnterCarPath(int index, Action onComplete) {
		if (index >= enterPath.Length) {
			onComplete();
			return;
		}
		enterPath[index].SetCar(aiCar, () => {
			if (index == 0) {
				aiCar.MaxSpeed = 20f;
			}
			SetEnterCarPath(index + 1, onComplete);
		}, () => true);
	}

	private AICar CreateCar() {
		AICar carPrefab = Settings.Instance.aiCarPrefabs[4];
		AICar car = ObjectPoolManager.Get(carPrefab, TrackGenerator.Instance.transform);
		car.name = carPrefab.name;
		initCarPosY = carPrefab.transform.position.y;
		initCarSpeed = carPrefab.MaxSpeed;
		return car;
	}
}