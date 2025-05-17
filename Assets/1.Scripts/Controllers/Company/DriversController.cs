using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DriversController : MonoBehaviour {
	
	[SerializeField] private Transform spawnPoints;
	[SerializeField] private List<Driver> drivers;
	[SerializeField] private Transform defaultTargetPointsParent;
	[SerializeField] private Transform taxiTargetPointsParent;
	
	private Transform cameraTransform;
	private DriverTargetPoint[] defaultTargetPoints;
	
	public void Init(Transform cameraTransform) {
		this.cameraTransform = cameraTransform;
		defaultTargetPoints = defaultTargetPointsParent.GetComponentsInChildren<DriverTargetPoint>();
	}
	
	public void SpawnDrivers() {
		DriverData[] driversData = SelectDrivers();
		for (int i = 0; i < driversData.Length; i++) {
			TryCreateDriver(driversData[i]);
		}
	}

	private void TryCreateDriver(DriverData driverData) {
		Driver driver = (Driver)ObjectPoolManager.Get(Resources.Load<Element>("Company/Driver"), transform);
		driver.name = $"Driver{drivers.Count}";
		driver.transform.position = GetMostDistantSpawnPoint().position;
		driver.gameObject.SetActive(true);
		driver.Init(driverData, cameraTransform);
		drivers.Add(driver);
		NavigateDriverToRandomPoint(driver);
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
		if (!driver.AvailableForRandomPoint || !drivers.Contains(driver)) {
			return;
		}
		
		List<DriverTargetPoint> list = new(defaultTargetPoints);
		list.RemoveAll(item => !item.IsAvailable);
		if (list.Count == 0) {
			driver.Wait(2f, () => NavigateDriverToRandomPoint(driver));
			return;
		}
		
		DriverTargetPoint targetPoint = list[Random.Range(0, list.Count)];
		targetPoint.IsAvailable = false;
		
		driver.SetTargetPoint(targetPoint.transform, () => {
			driver.AvailableForExit = false;
			driver.ShowBubble(targetPoint.GetBubbleText());
			driver.Wait(5f, () => {
				driver.HideBubble();
				NavigateDriverToRandomPoint(driver);
				targetPoint.IsAvailable = true;
				driver.AvailableForExit = true;
			});
		});
	}
	
	public void DriverReached(DriverData driverData, int parkingSlotIndex) {
		TryCreateDriver(driverData);
		drivers[^1].AvailableForExit = false;
		drivers[^1].transform.position = taxiTargetPointsParent.GetChild(parkingSlotIndex).position;
	}

	public bool TryGetDriverForExit(out Driver driver, out int parkingSlotIndex) {
		driver = null;
		parkingSlotIndex = -1;
		List<(Driver, int)> list = new();
		for (int i = 0; i < drivers.Count; i++) {
			if (drivers[i].AvailableForExit && PlayerPrefsManager.UserData.TryGetParkingSlotIndex(drivers[i].GetDriverData(), out int index)) {
				list.Add((drivers[i], index));
			}
		}
		if (list.Count == 0) {
			return false;
		}
		int randomDriverIndex = Random.Range(0, list.Count);
		driver = list[randomDriverIndex].Item1;
		parkingSlotIndex = list[randomDriverIndex].Item2;
		return true;
	}
	
	public void NavigateDriverToParkingSlot(Driver driver, int parkingSlotIndex, Action onComplete) {
		driver.AvailableForRandomPoint = false;
		driver.SetTargetPoint(taxiTargetPointsParent.GetChild(parkingSlotIndex), () => {
			drivers.Remove(driver);
			ObjectPoolManager.Release(driver);
			onComplete();
		});
	}

	public void ClearDrivers() {
		for (int i = 0; i < drivers.Count; i++) {
			ObjectPoolManager.Release(drivers[i]);
		}
		drivers.Clear();
		for (int i = 0; i < defaultTargetPoints.Length; i++) {
			defaultTargetPoints[i].IsAvailable = true;
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
			if (!selectedDrivers.Contains(drivers[i]) && drivers[i].hired) {
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
