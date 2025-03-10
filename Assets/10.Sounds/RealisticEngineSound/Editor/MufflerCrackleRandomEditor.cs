//______________________________________________//
//___________Realistic Engine Sounds____________//
//______________________________________________//
//_______Copyright © 2023 Skril Studio__________//
//______________________________________________//
//__________ http://skrilstudio.com/ ___________//
//______________________________________________//
//________ http://fb.com/yugelmobile/ __________//
//______________________________________________//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;

namespace SkrilStudio
{
    [CustomEditor(typeof(MufflerCrackleRandom))]
    public class MufflerCrackleRandomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var mcr = target as MufflerCrackleRandom;
            mcr.masterVolume = EditorGUILayout.Slider(new GUIContent("Master Volume", "Sets the maximum volume audio source can get in this prefab."), mcr.masterVolume, 0, 1);
            mcr.minDistance = EditorGUILayout.FloatField(new GUIContent("Minimum Distance", "Within the MinDistance, the sound will stay at loudest possible. Outside MinDistance it will begin to attenuate. Increase the MinDistance of a sound to make it ‘louder’ in a 3d world, and decrease it to make it ‘quieter’ in a 3d world."), mcr.minDistance);
            mcr.maxDistance = EditorGUILayout.FloatField(new GUIContent("Maximum Distance", "The distance where the sound stops attenuating at. Beyond this point it will stay at the volume it would be at MaxDistance units from the listener and will not attenuate any more."), mcr.maxDistance);
            mcr.audioMixer = EditorGUILayout.ObjectField(new GUIContent("Audio Mixer", "Optional. You can use a separe audio mixer for cracle sounds, or use the engine sound prefab's audio mixer."), mcr.audioMixer, typeof(AudioMixerGroup), true) as AudioMixerGroup;
            mcr.pitchMultiplier = EditorGUILayout.Slider(new GUIContent("Pitch multiplier", "Multiplies the audio source pitch with this value."), mcr.pitchMultiplier, 0.5f, 2f);
            mcr.playTime = EditorGUILayout.Slider(new GUIContent("Play TIme", "Crackle sound will be played for this long in secconds."), mcr.playTime, 0.5f, 4);
            mcr.playDuringShifting = EditorGUILayout.ToggleLeft(new GUIContent("Play During Shifting", "Will play cracle sounds while the vehicle is changing gears."), mcr.playDuringShifting, GUILayout.Width(150));
            mcr.playWithRevLimiter = EditorGUILayout.ToggleLeft(new GUIContent("Play With Revlimiter", "Will play cracle sounds the revlimiter."), mcr.playWithRevLimiter, GUILayout.Width(150));
            mcr.randomlyNotPlaying = EditorGUILayout.ToggleLeft(new GUIContent("Randomly not play any sound", "Randomly it will not play any sound when normally it would play."), mcr.randomlyNotPlaying, GUILayout.Width(190));
            mcr.isDelayRandom = EditorGUILayout.ToggleLeft(new GUIContent("Random Start Delay", "Randomize crackle sound's delay time."), mcr.isDelayRandom, GUILayout.Width(150));
            if (!mcr.isDelayRandom)
            {
                mcr.startDelay = EditorGUILayout.Slider(new GUIContent("Start Delay", "Crackle sound playing will be delayed for this secconds."), mcr.startDelay, 0, 2);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Min", GUILayout.Width(26));
                mcr.minDelayValue = EditorGUILayout.FloatField(mcr.minDelayValue, GUILayout.Width(65));
                EditorGUILayout.MinMaxSlider(ref mcr.minDelayValue, ref mcr.maxDelayValue, 0.1f, 2f);
                mcr.maxDelayValue = EditorGUILayout.FloatField(mcr.maxDelayValue, GUILayout.Width(65));
                EditorGUILayout.LabelField("Max", GUILayout.Width(26));
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
