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
		DriverData[] driversData = SelectDrivers();
		for (int i = driversData.Length - 1; i >= 0; i--) {
			if (driversData[i].hired) {
				TryCreateDriver(driversData[i]);
			}
		}
	}

	private void TryCreateDriver(DriverData driverData) {
		if (drivers.Count >= 8) {
			return;
		}
		Driver driver = (Driver)ObjectPoolManager.Get(Resources.Load<Element>("Company/Driver"), transform);
		driver.name = $"Driver{drivers.Count}";
		driver.transform.position = GetMostDistantSpawnPoint().position;
		driver.gameObject.SetActive(true);
		driver.Init(driverData, cameraTransform);
		NavigateDriverToRandomPoint(driver);
		drivers.Add(driver);
	}
	
	public void OnHireFireDriver(DriverData driverData) {
		if (driverData.hired) {
			TryCreateDriver(driverData);
		} else {
			if (drivers.Find(d => d.GetDriverData().design.id == driverData.design.id) is { } driver) {
				drivers.Remove(driver);
				ObjectPoolManager.Release(driver);
			}
		}
	}
	
	private void NavigateDriverToRandomPoint(Driver driver) {
		List<DriverTargetPoint> list = new(driverTargetPoints);
		list.RemoveAll(item => !item.IsAvailable);
		if (list.Count == 0) {
			driver.Wait(2f, () => NavigateDriverToRandomPoint(driver));
			return;
		}
		
		DriverTargetPoint targetPoint = list[Random.Range(0, list.Count)];
		targetPoint.IsAvailable = false;
		
		driver.SetTargetPoint(targetPoint, () => {
			driver.ShowBubble(targetPoint.GetBubbleText());
			driver.Wait(5f, () => {
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

	private static DriverData[] SelectDrivers() {
		List<DriverData> selectedDrivers = new();
		DriverData[] drivers = PlayerPrefsManager.UserData.drivers;
		for (int i = drivers.Length - 1; i >= 0; i--) {
			if (drivers[i].hired && PlayerPrefsManager.UserData.TryGetParkingSlotIndex(drivers[i], out _)) {
				selectedDrivers.Add(drivers[i]);
			}
		}
		for (int i = drivers.Length - 1; i >= 0; i--) {
			if (drivers[i].hired) {
				selectedDrivers.Add(drivers[i]);	
			}
		}
		return selectedDrivers.ToArray();
	}
	
	private Transform GetMostDistantSpawnPoint() {
		if (drivers.Count == 0) {
			return spawnPoints.GetChild(Random.Range(0, spawnPoints.childCount));
		}
		Transform bestSpawnPoint = null;
		float maxMinDistance = -1f;
		foreach (Transform spawnPoint in spawnPoints) {
			float minDistanceToCharacters = float.MaxValue;
			foreach (Driver driver in drivers) {
				if (!driver.gameObject.activeSelf) {
					continue;
				}
				float dist = Vector3.Distance(spawnPoint.position, driver.transform.position);
				if (dist < minDistanceToCharacters) {
					minDistanceToCharacters = dist;
				}
			}
			if (minDistanceToCharacters > maxMinDistance) {
				maxMinDistance = minDistanceToCharacters;
				bestSpawnPoint = spawnPoint;
			}
		}
		return bestSpawnPoint;
	}
}
