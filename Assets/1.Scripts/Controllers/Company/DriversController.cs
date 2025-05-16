using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DriversController : MonoBehaviour {
	
	[SerializeField] private Transform spawnPoints;
	[SerializeField] private List<Driver> drivers;
	[SerializeField] private Transform targetPoints;
	
	private Transform cameraTransform;
	private DriverTargetPoint[] driverTargetPoints;
	
	public void Init(Transform cameraTransform) {
		this.cameraTransform = cameraTransform;
		driverTargetPoints = targetPoints.GetComponentsInChildren<DriverTargetPoint>();
	}
	
	public void SpawnDrivers() {
		List<Transform> points = new();
		foreach (Transform spawnPoint in spawnPoints) {
			points.Add(spawnPoint);
		}
		for (int i = 0; i < 8; i++) {
			int randomIndex = Random.Range(0, points.Count);
			drivers.Add((Driver)ObjectPoolManager.Get(Resources.Load<Element>("Company/Driver"), transform));
			drivers[^1].name = $"Driver{drivers.Count - 1}";
			drivers[^1].transform.position = points[randomIndex].position;
			drivers[^1].gameObject.SetActive(true);
			drivers[^1].Init(cameraTransform);
			points.RemoveAt(randomIndex);
			NavigateDriverToRandomPoint(drivers[^1]);
		}
	}
	
	private void NavigateDriverToRandomPoint(Driver driver) {
		List<DriverTargetPoint> list = new(driverTargetPoints);
		list.RemoveAll(item => !item.IsAvailable);
		if (list.Count == 0) {
			this.Wait(2f, () => NavigateDriverToRandomPoint(driver));
			return;
		}
		
		DriverTargetPoint targetPoint = list[Random.Range(0, list.Count)];
		targetPoint.IsAvailable = false;
		
		driver.SetTargetPoint(targetPoint, () => {
			driver.ShowBubble(targetPoint.GetBubbleText());
			this.Wait(5f, () => {
				driver.HideBubble();
				NavigateDriverToRandomPoint(driver);
				targetPoint.IsAvailable = true;
			});
		});
	}

	public void ClearDrivers() {
		for (int i = 0; i < drivers.Count; i++) {
			ObjectPoolManager.Release(drivers[i]);
		}
		drivers.Clear();
		for (int i = 0; i < driverTargetPoints.Length; i++) {
			driverTargetPoints[i].IsAvailable = true;
		}
		StopAllCoroutines();
	}
}
