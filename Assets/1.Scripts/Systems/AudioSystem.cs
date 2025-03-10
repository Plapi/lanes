using System;
using UnityEngine;
using UnityEngine.Audio;

public static class AudioSystem {
	
	private static Mixer[] mixers;
	private static MonoBehaviour monoBehaviour;

	public static void Init(MonoBehaviour behaviour, float[] volumes) {
		monoBehaviour = behaviour;
		mixers = new Mixer[volumes.Length];
		Utils.EnumerateEnum<MixerType>((mixerType, index) => {
			if (mixerType != MixerType.CarEngine) {
				mixers[index] = new Mixer(mixerType.ToString(), volumes[index]);	
			}
		});
	}
	
	public static AudioSource Play(AudioClip audioClip, MixerType mixerType = MixerType.Effects, Action onComplete = null) {
		if (audioClip == null) {
			onComplete?.Invoke();
			return null;
		}
		AudioSource audioSource = SetAudioSourceComponent(new GameObject($"OneShotSound_{audioClip.name}"), mixerType);
		audioSource.volume = 1f;
		audioSource.PlayOneShot(audioClip);
		UnityEngine.Object.Destroy(audioSource.gameObject, audioClip.length + 0.5f);
		if (onComplete != null) {
			monoBehaviour.Wait(audioClip.length, onComplete);	
		}
		return audioSource;
	}
	
	public static AudioSource SetAudioSourceComponent(GameObject gameObject, MixerType mixerType) {

		if (gameObject == null) {
			return null;
		}

		if (mixers == null) {
			return gameObject.TryGetComponent(out AudioSource source) ? source : gameObject.AddComponent<AudioSource>();
		}

		AudioMixerGroup audioMixerGroup = mixers[(int)mixerType].audioMixerGroup;
		AudioSource audioSource = null;
		AudioSource[] audioSources = gameObject.GetComponents<AudioSource>();
		for (int i = 0; i < audioSources.Length; i++) {
			if (audioSources[i].outputAudioMixerGroup == null || audioSources[i].outputAudioMixerGroup == audioMixerGroup) {
				audioSource = audioSources[i];
			}
		}

		audioSource = audioSource != null ? audioSource : gameObject.AddComponent<AudioSource>();
		audioSource.outputAudioMixerGroup = audioMixerGroup;
		return audioSource;
	}
	
	public static void UpdateVolume(MixerType mixerType, float volume) {
		mixers?[(int)mixerType].UpdateVolume(volume);
	}
	
	public static float GetVolume(MixerType mixerType) {
		return mixers != null ? mixers[(int)mixerType].currentVolume : 0;
	}
}

public struct Mixer {
		
	public AudioMixer audioMixer { get; private set; }
	public AudioMixerGroup audioMixerGroup { get; private set; }
	public float currentVolume { get; private set; }

	public Mixer(string name, float volume) {
		audioMixer = Resources.Load<AudioMixer>("AudioMixers/" + name);
		audioMixerGroup = audioMixer.FindMatchingGroups("Master")[0];
		currentVolume = volume;
		UpdateVolume();
	}
		
	public void UpdateVolume(float volume) {
		currentVolume = volume;
		UpdateVolume();
	}
		
	public void UpdateVolume() {
		if (audioMixer == null) {
			return;
		}
		audioMixer.SetFloat("Volume", Mathf.Log10(Mathf.Clamp(currentVolume, 0.0001f, 1f)) * 20f + currentVolume);
	}
}

public enum MixerType {
	Effects,
	Music,
	CarEngine
}
