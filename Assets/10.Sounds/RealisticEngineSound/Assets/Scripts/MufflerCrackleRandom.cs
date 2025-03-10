//______________________________________________//
//___________Realistic Engine Sounds____________//
//______________________________________________//
//_______Copyright © 2024 Skril Studio__________//
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
    public class MufflerCrackleRandom : MonoBehaviour
    {
        RealisticEngineSound res;
        // master volume setting
        [HideInInspector]
        public float masterVolume = 1f;
        //
        [HideInInspector]
        public bool playDuringShifting = true;
        [HideInInspector]
        public bool playWithRevLimiter = true;
        // audio mixer
        [HideInInspector]
        public AudioMixerGroup audioMixer;
        private AudioMixerGroup _audioMixer;
        // pitch multiplier
        [HideInInspector]
        public float pitchMultiplier = 1;
        // play time
        [HideInInspector]
        public float playTime = 2;
        private float playTime_;
        private WaitForSeconds _playtime;

        // play start delay
        [HideInInspector]
        public float startDelay = 0.5f;
        private float startDelay_;
        private WaitForSeconds _startDelay;
        // start delay random
        [HideInInspector]
        public bool isDelayRandom = false;
        [HideInInspector]
        public float minDelayValue = 0.25f;
        [HideInInspector]
        public float maxDelayValue = 1.0f;
        [HideInInspector]
        public float minDistance = 2;
        [HideInInspector]
        public float maxDistance = 250;

        // randomly not playing the sound
        [HideInInspector]
        public bool randomlyNotPlaying = false;
        private bool waitForRandomNotPlay = false; // used to prevent isGoingToPlay getting a random value multiple times during one play session
        private int isGoingToPlay = 0;

        // audio clips
        public AudioClip[] crackleClip;
        // audio sources
        private AudioSource crackleSound;
        // curve settings
        public AnimationCurve crackleVolCurve;
        // private
        private float clipsValue;
        private int oneShotController = 0;
        private bool isGasPedalPressing = false;
        private bool isShifting = false;
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
                // use engine sound's audio mixer for this prefab
                _audioMixer = res.audioMixer;
                audioMixer = _audioMixer;
            }
            playTime_ = playTime;
            startDelay_ = startDelay;
            UpdateWaitTime();
        }
        void Update()
        {
            DynamicSoundController.AudioValueControll(audioFloats, minDistance, maxDistance, res.dopplerLevel, res.spatialBlend);
            DynamicSoundController.DynamicSoundValues(dynamicFloats, clipsValue, masterVolume, res.engineLoad, pitchMultiplier, res.optimisationLevel, 1, 1, 1);
            clipsValue = DynamicSoundController.BasicClipsValue(res.engineCurrentRPM, res.maxRPMLimit);
            if (playDuringShifting)
            {
                isGasPedalPressing = res.gasPedalPressing;
            }
            else // prevent playing muffler crackle sounds even when gas pedal is released during shifting
            {
                if (isShifting)
                    isGasPedalPressing = true;
                else
                    isGasPedalPressing = res.gasPedalPressing;
            }
            isShifting = res.isShifting;
            if (res.enabled)
                crackleSound = DynamicSoundController.MufflerCrackleRandom(gameObject, crackleSound, crackleClip, res.audioRolloffMode, res.audioVelocityUpdateMode, audioMixer, crackleVolCurve, res.optimiseAudioSources, dynamicFloats, audioFloats, playDuringShifting, isGasPedalPressing, isShifting, res.isAudible, oneShotController);
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
            if (startDelay_ != startDelay) // playTime value is got changed on runtime
                UpdateStartDelayTime();
            if (isDelayRandom)
                UpdateStartDelayTime();
            if (crackleSound != null && crackleSound.loop) // an interesting way to share bool value from another script. Audio source's loop bool is used to StartCoroutine(StartDelayCrackle()) from another script
            {
                StartCoroutine(StartDelayCrackle());
                crackleSound.loop = false;
            }
            if (playWithRevLimiter && res.maxRPM != null && res.maxRPMVolCurve.Evaluate(clipsValue) > 0.9f) // rev limiter playing
            {
                oneShotController = 2;
                if (crackleSound != null && !crackleSound.isPlaying)
                {
                    crackleSound.clip = crackleClip[Random.Range(0, crackleClip.Length)];
                    crackleSound.pitch = pitchMultiplier;
                    crackleSound.volume = crackleVolCurve.Evaluate(clipsValue) * masterVolume;
                    crackleSound.Play();                   
                }
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
        private void UpdateStartDelayTime()
        {
            if (!isDelayRandom)
            {
                _startDelay = new WaitForSeconds(startDelay);
                startDelay_ = startDelay;
            }
            else
            {
                _startDelay = new WaitForSeconds(Random.Range(minDelayValue, maxDelayValue));
                startDelay_ = startDelay;
            }
        }
        public IEnumerator StartDelayCrackle()
        {
            while (true)
            {
                yield return _startDelay;
                // play sound
                if (oneShotController != 0 && crackleSound != null && !crackleSound.isPlaying)
                {                   
                    if (randomlyNotPlaying)
                    {
                        if (!waitForRandomNotPlay)
                        {
                            isGoingToPlay = Random.Range(0, 4); // increase number 4 to have higher chance of playing the sound
                            waitForRandomNotPlay = true;
                        }
                        if(isGoingToPlay > 0) // randomly choosed to play a sound
                        {
                            crackleSound.clip = crackleClip[Random.Range(0, crackleClip.Length)];
                            crackleSound.Play();
                        }
                    }
                    else
                    {
                        crackleSound.clip = crackleClip[Random.Range(0, crackleClip.Length)];
                        crackleSound.Play();
                    }
                }
                StartCoroutine(WaitForCrackle());
                break;
            }
        }
        IEnumerator WaitForCrackle()
        {
            while (true)
            {
                yield return _playtime; // destroy audio playtime secconds later
                oneShotController = 0;
                waitForRandomNotPlay = false;
                if (res.optimiseAudioSources == DynamicSoundController.OptimiseAudioSources.Destroy && crackleSound != null)
                    Destroy(crackleSound);
                else
                    crackleSound.Pause();
                break;
            }
        }
    }
}