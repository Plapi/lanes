//______________________________________________//
//___________Realistic Engine Sounds____________//
//______________________________________________//
//_______Copyright © 2022 Skril Studio__________//
//______________________________________________//
//__________ http://skrilstudio.com/ ___________//
//______________________________________________//
//________ http://fb.com/yugelmobile/ __________//
//______________________________________________//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using SkrilStudio;

namespace SkrilStudio
{
    public class ShiftingSound : MonoBehaviour
    {
        RealisticEngineSound res;
        // master volume setting
        [Range(0.1f, 1.0f)]
        public float masterVolume = 1f;
        // audio mixer
        public AudioMixerGroup audioMixer;
        private AudioMixerGroup _audioMixer;
        public float minDistance = 1;
        public float maxDistance = 50;
        public AudioClip shiftingSoundClip;
        private AudioSource shiftingSound;
        private int playOnce = 0;
        [Range(0.2f, 1f)]
        public float playTime = .4f;
        private float playTime_;
        private WaitForSeconds _playtime;
        void Start()
        {
            res = gameObject.transform.parent.GetComponent<RealisticEngineSound>();
            // audio mixer settings
            if (audioMixer != null) // user is using a seperate audio mixer for this prefab
            {
                _audioMixer = audioMixer;
            }
            else
            {
                if (res.audioMixer != null) // use engine sound's audio mixer for this prefab
                {
                    _audioMixer = res.audioMixer;
                    audioMixer = _audioMixer;
                }
            }
            playTime_ = playTime;
            UpdateWaitTime();
        }
        void Update()
        {
            if (res.enabled)
            {
                if (res.isAudible)
                {
                    if (playTime_ != playTime) // playTime value is got changed on runtime
                        UpdateWaitTime();
                    if (res.isShifting && playOnce == 0)
                    {
                        if (shiftingSound == null)
                            CreateShiftSound();
                        else
                        {
                            shiftingSound.volume = masterVolume;
                            shiftingSound.Play();
                        }
                        StartCoroutine(WaitForShifting());
                        playOnce = 1;
                    }
                }
                else
                {
                    playOnce = 0;
                    if (shiftingSound != null)
                    {
                        if (res.optimiseAudioSources == DynamicSoundController.OptimiseAudioSources.Destroy)
                            Destroy(shiftingSound);
                        else
                            shiftingSound.Pause();
                    }
                }
            }
            else
            {
                if (shiftingSound != null)
                    Destroy(shiftingSound);
            }
        }
#if UNITY_EDITOR
        private void LateUpdate()
        {
            // velocity update mode got changed on runtime, remake all audio sources
            if (shiftingSound != null && shiftingSound.velocityUpdateMode != res.audioVelocityUpdateMode)
                Destroy(shiftingSound);
        }
#endif
        private void OnEnable() // if prefab got new audiomixer on runtime, it will use that after prefab got re-enabled
        {
            Start();
        }
        private void OnDisable()
        {
            if (shiftingSound != null)
                Destroy(shiftingSound);
        }
        void CreateShiftSound()
        {
            shiftingSound = gameObject.AddComponent<AudioSource>();
            shiftingSound.rolloffMode = res.audioRolloffMode;
            shiftingSound.minDistance = minDistance;
            shiftingSound.maxDistance = maxDistance;
            shiftingSound.spatialBlend = res.spatialBlend;
            shiftingSound.dopplerLevel = res.dopplerLevel;
            shiftingSound.volume = masterVolume;
            shiftingSound.velocityUpdateMode = res.audioVelocityUpdateMode;
            if (_audioMixer != null)
                shiftingSound.outputAudioMixerGroup = _audioMixer;
            shiftingSound.clip = shiftingSoundClip;
            shiftingSound.pitch = 1;
            shiftingSound.loop = false;
            shiftingSound.Play();
        }
        private void UpdateWaitTime()
        {
            _playtime = new WaitForSeconds(playTime);
            playTime_ = playTime;
        }
        IEnumerator WaitForShifting()
        {
            while (true)
            {
                yield return _playtime; // destroy or pause audio playtime secconds later                
                if (shiftingSound != null)
                {
                    if (res.optimiseAudioSources == DynamicSoundController.OptimiseAudioSources.Destroy)
                        Destroy(shiftingSound);
                    else
                        shiftingSound.Pause();
                }
                playOnce = 0;
                break;
            }
        }
    }
}
