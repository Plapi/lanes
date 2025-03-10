using SkrilStudio;
using UnityEngine;

public class TestRealisticEngineSound : MonoBehaviour {
	
	[SerializeField] private RealisticEngineSound realisticEngineSound;

	[SerializeField] private bool gasPedalPressing;
	[SerializeField] [Range(0, 7000)] private float engineCurrentRPM;
	
	private void Start() {
		realisticEngineSound.carMaxSpeed = 60;
	}

	private void Update() {
		realisticEngineSound.gasPedalPressing = gasPedalPressing;
		realisticEngineSound.engineCurrentRPM = engineCurrentRPM;
	}
}
