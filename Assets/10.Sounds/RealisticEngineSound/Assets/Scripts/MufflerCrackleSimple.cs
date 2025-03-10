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
    public class MufflerCrackleSimple : MonoBehaviour
    {
        RealisticEngineSound res;
        // master volume setting
        [Range(0.1f, 1.0f)]
        public float masterVolume = 1f;
        public float minDistance = 2;
        public float maxDistance = 250;
        // audio mixer
        public AudioMixerGroup audioMixer;
        private AudioMixerGroup _audioMixer;
        // pitch multiplier
        [Range(0.5f, 2.0f)]
        public float pitchMultiplier = 1;
        // play time
        [Range(0.5f, 4)]
        public float playTime = 2;
        private float playTime_;
        // audio clips
        public AudioClip crackleClip;
        private AudioClip _crackleClip;
        // audio sources
        private AudioSource crackleSound;
        // curve settings
        public AnimationCurve crackleVolCurve;
        // private
        private float clipsValue;
        private int oneShotController = 0;
        private WaitForSeconds _playtime;
        private bool isGasPedalPressing = false;
        private float[] audioFloats = new float[4];
        private float[] dynamicFloats = new float[8];
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
            DynamicSoundController.AudioValueControll(audioFloats, minDistance, maxDistance, res.dopplerLevel, res.spatialBlend);
            DynamicSoundController.DynamicSoundValues(dynamicFloats, clipsValue, masterVolume, res.engineLoad, pitchMultiplier, res.optimisationLevel, 1, 1, 1);
            clipsValue = DynamicSoundController.BasicClipsValue(res.engineCurrentRPM, res.maxRPMLimit);
            isGasPedalPressing = res.gasPedalPressing;
            if (res.enabled)
                crackleSound = DynamicSoundController.MufflerCrackleSimple(gameObject, crackleSound, crackleClip, res.audioRolloffMode, res.audioVelocityUpdateMode, audioMixer, crackleVolCurve, res.optimiseAudioSources, dynamicFloats, audioFloats, isGasPedalPressing, res.isAudible, oneShotController);
            else
            {
                if (crackleSound != null)
                    Destroy(crackleSound);
            }
            // one shot controller
            if (isGasPedalPressing)
            {
                if (oneShotController != 2)
                    oneShotController = 1; // prepare for one shoot
            }
            else
            {
                if (oneShotController == 1)
                    oneShotController = 2;
            }
            if (playTime_ != playTime) // playTime value is got changed on runtime
                UpdateWaitTime();
            if (crackleSound != null && crackleSound.loop) // an interesting way to share bool value from another script. Audio source's loop bool is used to StartCoroutine(WaitForCrackle()) from another script
            {
                StartCoroutine(WaitForCrackle());
                crackleSound.loop = false;
                crackleSound.Play();
            }
        }
#if UNITY_EDITOR
        private void LateUpdate()
        {
            // velocity update mode got changed on runtime, remake all audio sources
            if (crackleSound != null && crackleSound.velocityUpdateMode != res.audioVelocityUpdateMode)
                Destroy(crackleSound);
        }
#endif
        private void OnEnable() // if prefab got new audiomixer on runtime, it will use that after prefab got re-enabled
        {
            Start();
        }
        private void OnDisable()
        {
            if (crackleSound != null)
                Destroy(crackleSound);
        }
        private void UpdateWaitTime()
        {
            _playtime = new WaitForSeconds(playTime);
            playTime_ = playTime;
        }
        IEnumerator WaitForCrackle()
        {
            while (true)
            {
                yield return _playtime; // destroy audio playtime secconds later
                oneShotController = 0;
                if (res.optimiseAudioSources == DynamicSoundController.OptimiseAudioSources.Destroy && crackleSound != null)
                    Destroy(crackleSound);
                else
                    crackleSound.Pause();
                break;
            }
        }
    }
}
