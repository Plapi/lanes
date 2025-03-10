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
using UnityEditor;
using UnityEngine.Audio;
using UnityEngine.Networking;
using UnityEditor.SceneManagement;
using SkrilStudio;

namespace SkrilStudio
{
    [CustomEditor(typeof(RealisticEngineSound))]
    public class RealisticEngineSoundEditor : Editor
    {
        private Texture texLogo;
        private Material material;
        bool isInterior;
        float high = 1;
        bool editCurves = false;
        bool editClips = false;
        bool editSettings = false;
        bool about = false;
        static bool debugInfo = false;
        static bool _debugInfo = false; // for check saved state
        bool hideVolumeCurves = false;
        static bool showCurvesWindow = true;
        static int inspectorPreviousState = 0;
        bool _isDynamicMixer = false;
        bool enableReverseGear = false;
        enum ReverseGearSFX { Off, On }
        ReverseGearSFX _reverseGearSFX = new ReverseGearSFX();
        bool enableReverseBeep = false;
        enum ReverseBeepSFX { Off, On }
        ReverseBeepSFX _reverseBeepSFX = new ReverseBeepSFX();
        static bool enableNotify = true;
        static bool _enableNotify = true; // for check saved state
        enum ReverseGearClipType { Preset, Custom }
        ReverseGearClipType reverseGearClipType = new ReverseGearClipType();
        enum ReverseBeepClipType { Preset, Custom }
        ReverseBeepClipType reverseBeepClipType = new ReverseBeepClipType();
        enum WindClipType { Preset, Custom }
        WindClipType windClipType = new WindClipType();
        enum CameraType { Exterior, Interior }
        CameraType cameraType = new CameraType();
        enum PrefabType { Player, Opponent, Custom }
        PrefabType prefabType = new PrefabType();
        private GUIStyle changeTextSizeGuiStyle = new GUIStyle();
        private GUIStyle redTextSizeGuiStyle = new GUIStyle();
        private const string resFolderGUID = "7938cc6ec3661c9428da44ff3ff47171";
        private void OnEnable()
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(resFolderGUID);
            texLogo = (Texture2D)AssetDatabase.LoadAssetAtPath(assetPath + "/Editor/logo.png", typeof(Texture2D));
            // Find the "Hidden/Internal-Colored" shader, and cache it for use.
            material = new Material(Shader.Find("Hidden/Internal-Colored"));
            enableNotify = EditorPrefs.GetBool("enableNotify");
            _enableNotify = enableNotify;
            debugInfo = EditorPrefs.GetBool("editorDebug");
            _debugInfo = debugInfo;
            // check for curve editor settings
            inspectorPreviousState = EditorPrefs.GetInt("inspectorPreviousState");
            showCurvesWindow = EditorPrefs.GetBool("curvesWindowState");
            if (inspectorPreviousState == 0)
            {
                editCurves = false;
                editClips = false;
                editSettings = false;
                about = false;
            }
            if (inspectorPreviousState == 1)
            {
                editCurves = true;
                editClips = false;
                editSettings = false;
                about = false;
            }
            if (inspectorPreviousState == 2)
            {
                editCurves = false;
                editClips = true;
                editSettings = false;
                about = false;
            }
            if (inspectorPreviousState == 3)
            {
                editCurves = false;
                editClips = false;
                editSettings = true;
                about = false;
            }
            if (inspectorPreviousState == 4)
            {
                editCurves = false;
                editClips = false;
                editSettings = false;
                about = true;
            }
        }
        public Rect logoRect;
        public override void OnInspectorGUI()
        {
            var res = target as RealisticEngineSound;
            string assetPath = AssetDatabase.GUIDToAssetPath(resFolderGUID);
            //DrawDefaultInspector();
            // check for editor setting is changed
            if (enableNotify != _enableNotify)
                SaveEditorSettings();
            if (debugInfo != _debugInfo)
                SaveEditorSettings();
            // logo
            logoRect.height = texLogo.height;
            logoRect.width = texLogo.width;
            GUILayout.BeginHorizontal();
            GUILayout.Space((EditorGUIUtility.currentViewWidth - texLogo.width - 10f) / 2f);
            GUILayout.Label(texLogo, GUILayout.Width(logoRect.width - 25f), GUILayout.Height(logoRect.height));
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel++;
            // end logo
            // set enum values      
            if (res.isInterior)
                cameraType = CameraType.Interior;
            else
                cameraType = CameraType.Exterior;
            if (res.prefabType == RealisticEngineSound.PrefabType.Player)
                prefabType = PrefabType.Player;
            if (res.prefabType == RealisticEngineSound.PrefabType.Opponent)
                prefabType = PrefabType.Opponent;
            if (res.prefabType == RealisticEngineSound.PrefabType.Custom)
                prefabType = PrefabType.Custom;
            // wind
            if (res.customWindClip)
                windClipType = WindClipType.Custom;
            else
                windClipType = WindClipType.Preset;
            // reverse
            if (res.customReverseClip)
                reverseGearClipType = ReverseGearClipType.Custom;
            else
                reverseGearClipType = ReverseGearClipType.Preset;
            // version
            RES2Editor.VersionNumber();

            GUI.backgroundColor = new Color(.7f, .7f, .7f);
            EditorGUILayout.BeginHorizontal(GUI.skin.button);
            // edit curves
            if (!editCurves)
                GUI.backgroundColor = new Color(.85f, .85f, .85f);
            else
                GUI.backgroundColor = new Color(1f, 1f, 1f);
            if (GUILayout.Button("Edit Curves"))
            {
                if (!editCurves)
                {
                    editCurves = true;
                    inspectorPreviousState = 1;
                }
                else
                {
                    editCurves = false;
                    inspectorPreviousState = 0;
                }
                editClips = false;
                editSettings = false;
                about = false;
                SaveInspectorState();
            }
            // edit clips
            if (!editClips)
                GUI.backgroundColor = new Color(.85f, .85f, .85f);
            else
                GUI.backgroundColor = new Color(1f, 1f, 1f);
            if (GUILayout.Button("Edit Motor Audio Clips"))
            {
                if (!editClips)
                {
                    editClips = true;
                    inspectorPreviousState = 2;
                }
                else
                {
                    editClips = false;
                    inspectorPreviousState = 0;
                }
                editCurves = false;
                editSettings = false;
                about = false;
                SaveInspectorState();
            }
            // edit settings
            if (!editSettings)
                GUI.backgroundColor = new Color(.85f, .85f, .85f);
            else
                GUI.backgroundColor = new Color(1f, 1f, 1f);
            if (GUILayout.Button("Edit Settings"))
            {
                if (!editSettings)
                {
                    editSettings = true;
                    inspectorPreviousState = 3;
                }
                else
                {
                    editSettings = false;
                    inspectorPreviousState = 0;
                }
                editCurves = false;
                editClips = false;
                about = false;
                SaveInspectorState();
            }
            // about
            if (!about)
                GUI.backgroundColor = new Color(.85f, .85f, .85f);
            else
                GUI.backgroundColor = new Color(1f, 1f, 1f);
            if (GUILayout.Button("About"))
            {
                if (!about)
                {
                    about = true;
                    inspectorPreviousState = 4;
                }
                else
                {
                    about = false;
                    inspectorPreviousState = 0;
                }
                editCurves = false;
                editClips = false;
                editSettings = false;
                SaveInspectorState();
            }
            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = new Color(.7f, .7f, .7f);
            RES2Editor.ProgressBar(res.engineCurrentRPM / res.maxRPMLimit, "Current RPM: " + Mathf.Round(res.engineCurrentRPM));
            RES2Editor.ProgressBar(res.engineLoad / 1, "Engine Load");
            if (res.enableAggressivness == RealisticEngineSound.AggressivnessEnum.On)
                RES2Editor.ProgressBar(res.aggressivnessMaster, "Aggressivness Fx Level");
            else
                RES2Editor.ProgressBar(0, "Aggressivness Fx is turned off");
            if (debugInfo)
            {
                if (res.gasPedalValueSetting == RealisticEngineSound.GasPedalValue.Simulated)
                {
                    if (res.gasPedalPressing)
                        EditorGUILayout.LabelField("Gas pedal is currently pressed on.");
                    else
                        EditorGUILayout.LabelField("Gas pedal is not pressed on currently.");
                }
                if (res.enableReverseGear)
                {
                    if (res.isReversing)
                        EditorGUILayout.LabelField("Vehicle is currently reversing.");
                    else
                        EditorGUILayout.LabelField("Vehicle is not reversing.");
                }

                if (res.isShifting)
                    EditorGUILayout.LabelField("Vehicle is changing gears.");
                else
                    EditorGUILayout.LabelField("Vehicle is not changing gears currently.");
                if (res.isAudible)
                    EditorGUILayout.LabelField("This engine sound is audible.");
                else
                    EditorGUILayout.LabelField("This engine sound is not audible.");
                EditorGUILayout.LabelField("Vehicle current speed: " + res.carCurrentSpeed);
                EditorGUILayout.LabelField("Vehicle max speed: " + res.carMaxSpeed);
                // X Y curve values
                float resClipsValue = (res.engineCurrentRPM / res.maxRPMLimit);
                EditorGUILayout.LabelField("Current Curve Point TIME = " + resClipsValue);
                float maxValue = Mathf.Max(res.idleVolCurve.Evaluate(resClipsValue), res.idle_lowVolCurve.Evaluate(resClipsValue), res.lowVolCurve.Evaluate(resClipsValue), res.low_medVolCurve.Evaluate(resClipsValue), res.medVolCurve.Evaluate(resClipsValue), res.med_highVolCurve.Evaluate(resClipsValue), res.highVolCurve.Evaluate(resClipsValue), res.very_highVolCurve.Evaluate(resClipsValue));
                // on load y curve value
                if (res.curvesByVolume == RealisticEngineSound.CurvesByVolume.ShowWithOnLoadVolume)
                    EditorGUILayout.LabelField("Current Curve Point VALUE = " + maxValue * res.onLoadVolumeByRPM.Evaluate(resClipsValue));
                // off load y curve value
                if (res.curvesByVolume == RealisticEngineSound.CurvesByVolume.ShowWithOffLoadVolume)
                    EditorGUILayout.LabelField("Current Curve Point VALUE = " + maxValue * res.offLoadVolumeByRPM.Evaluate(resClipsValue));
                // raw y curve values
                if (res.curvesByVolume == RealisticEngineSound.CurvesByVolume.ShowRawCurves)
                    EditorGUILayout.LabelField("Current Curve Point VALUE = " + maxValue);
            }
            // Begin to draw a horizontal layout, using the helpBox EditorStyle
            GUILayout.BeginVertical(GUI.skin.button);
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(1f, 1f, 1f);

            if (showCurvesWindow)
            {
                res.curvesByVolume = (RealisticEngineSound.CurvesByVolume)EditorGUILayout.EnumPopup(res.curvesByVolume, GUILayout.Width(200));
                GUI.backgroundColor = new Color(.9f, .9f, .9f);
                if (GUILayout.Button("Hide Curves Window", GUILayout.Height(15)))
                {
                    showCurvesWindow = false;
                    SaveCurveWindowState();
                }
            }
            else
            {
                GUI.backgroundColor = new Color(.9f, .9f, .9f);
                if (GUILayout.Button("Show Curves Window"))
                {
                    showCurvesWindow = true;
                    SaveCurveWindowState();
                }
            }
            GUILayout.EndHorizontal();
            GUI.backgroundColor = new Color(.7f, .7f, .7f);
            Repaint(); // update faster the custom inspector
            if (showCurvesWindow)
            {
                if (res.soundLevels == DynamicSoundController.SoundLevels.Seven)
                    RES2Editor.DrawCurveWindow(high, material, hideVolumeCurves, res.maxRPMLimit, res.engineCurrentRPM, res.showCurvesVolumeRPM, res.showCurvesOffVolumeRPM, RES2Editor.SoundLevels.Seven, res.onLoadVolumeByRPM, res.offLoadVolumeByRPM, res.idleVolCurve, res.idle_lowVolCurve, res.lowVolCurve, res.low_medVolCurve, res.medVolCurve, res.med_highVolCurve, res.highVolCurve, res.very_highVolCurve);
                if (res.soundLevels == DynamicSoundController.SoundLevels.Three)
                    RES2Editor.DrawCurveWindow(high, material, hideVolumeCurves, res.maxRPMLimit, res.engineCurrentRPM, res.showCurvesVolumeRPM, res.showCurvesOffVolumeRPM, RES2Editor.SoundLevels.Three, res.onLoadVolumeByRPM, res.offLoadVolumeByRPM, res.idleVolCurve, res.idle_lowVolCurve, res.lowVolCurve, res.low_medVolCurve, res.medVolCurve, res.med_highVolCurve, res.highVolCurve, res.very_highVolCurve);
                if (res.soundLevels == DynamicSoundController.SoundLevels.One)
                    RES2Editor.DrawCurveWindow(high, material, hideVolumeCurves, res.maxRPMLimit, res.engineCurrentRPM, res.showCurvesVolumeRPM, res.showCurvesOffVolumeRPM, RES2Editor.SoundLevels.One, res.onLoadVolumeByRPM, res.offLoadVolumeByRPM, res.idleVolCurve, res.idle_lowVolCurve, res.lowVolCurve, res.low_medVolCurve, res.medVolCurve, res.med_highVolCurve, res.highVolCurve, res.very_highVolCurve);
            }
            // undo
            EditorGUI.BeginChangeCheck();
            GUILayout.EndVertical();
            // please write a review notify
            if (enableNotify)
            {
                enableNotify = RES2Editor.Notify(enableNotify, RES2Editor.RES2Versions.Lite);
            }
            if (about)
            {
                RES2Editor.About(RES2Editor.RES2Versions.Lite);
            }
            // audio clips
            GUI.backgroundColor = new Color(.6f, .6f, .6f);
            if (editClips)
            {
                editClips = true;
                EditorGUILayout.BeginVertical(GUI.skin.button);
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(1, 1, 1f);
                res.prefabName = EditorGUILayout.TextField("Engine Sound Pack's Folder Name: ", res.prefabName);
                GUI.backgroundColor = new Color(.6f, .6f, 1f);
                if (GUILayout.Button("Get From Prefab's Name"))
                {
                    res.prefabName = Selection.activeGameObject.name;
                    res.prefabName = RES2Editor.GetPrefabName(res.prefabName);
                    Debug.Log("Got prefab's name, and it is: " + Selection.activeGameObject.name);
                }
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = new Color(1, 1, 1f);
                res.parentFolder = EditorGUILayout.TextField(new GUIContent("Parent Folder Name: ", "Folder structure should be: YourProject/Assets/ -ParentFolder- or by default 'RealisticEngineSounds' /Assets/Audio/YourEngineSoundsName"), "" + res.parentFolder);

                GUI.backgroundColor = new Color(.6f, .6f, 1f);

                if (GUILayout.Button(new GUIContent("Load Exterior Audio Clips From ../" + res.prefabName + " Folder", "It will load those audio files of the engine sound pack that are made for exterior camera view.")))
                {
                    //res.enabled = false;
                    isInterior = false;
                    res.isInterior = false;
                    res.reversingClip = RES2Editor.ChangeReverseClip(res.reverseClipID, res.isInterior, res.reversingClip);
                    res.reversingBeepClip = RES2Editor.ChangeReverseBeepClip(res.reverseBeepClipID, res.isInterior, res.reversingBeepClip);
                    res.windNoiseClip = RES2Editor.ChangeWindClip(res.windClipID, res.isInterior, res.windNoiseClip);
                    LoadAudioClips();
                    res.masterVolume = 1;
                    res.windMasterVolume = 1;
                    res.DestroyAll();
                }
                if (GUILayout.Button(new GUIContent("Load Interior Audio Clips From ../" + res.prefabName + "/Interior or Helmet Folder", "It will load those audio files of the engine sound pack that are made for interior camera view.")))
                {
                    //res.enabled = false;
                    isInterior = true;
                    res.isInterior = true;
                    res.dynamicAudioMixer = RealisticEngineSound.DynamicAudioMixer.Off;
                    res.reversingClip = RES2Editor.ChangeReverseClip(res.reverseClipID, res.isInterior, res.reversingClip);
                    res.reversingBeepClip = RES2Editor.ChangeReverseBeepClip(res.reverseBeepClipID, res.isInterior, res.reversingBeepClip);
                    res.windNoiseClip = RES2Editor.ChangeWindClip(res.windClipID, res.isInterior, res.windNoiseClip);
                    LoadAudioClips();
                    res.masterVolume = 0.6f;
                    res.windMasterVolume = 0.4f;
                    res.DestroyAll();
                }
                if (res.soundLevels == DynamicSoundController.SoundLevels.Seven) // seven sound levels are used for engine sound
                {
                    if (res.offLoadType == RealisticEngineSound.OffLoadType.Prerecorded)
                        EditAudioClips(RES2Editor.SoundLevels.Seven, false); // off load is pre-recorded
                    else
                        EditAudioClips(RES2Editor.SoundLevels.Seven, true); // off load is simulated
                }
                if (res.soundLevels == DynamicSoundController.SoundLevels.Three) // three sound levels are used for engine sound
                {
                    if (res.offLoadType == RealisticEngineSound.OffLoadType.Prerecorded)
                        EditAudioClips(RES2Editor.SoundLevels.Three, false); // off load is pre-recorded
                    else
                        EditAudioClips(RES2Editor.SoundLevels.Three, true); // off load is simulated
                }
                if (res.soundLevels == DynamicSoundController.SoundLevels.One) // one sound levels are used for engine sound
                {
                    if (res.offLoadType == RealisticEngineSound.OffLoadType.Prerecorded)
                        EditAudioClips(RES2Editor.SoundLevels.One, false); // off load is pre-recorded
                    else
                        EditAudioClips(RES2Editor.SoundLevels.One, true); // off load is simulated
                }
                EditorGUILayout.EndVertical();
            }
            // end audio clips
            // begin curves
            if (editCurves)
            {
                if (res.soundLevels == DynamicSoundController.SoundLevels.Seven)
                    RES2Editor.EditCurves(RES2Editor.SoundLevels.Seven, res.useRPMLimit, res.onLoadVolumeByRPM, res.offLoadVolumeByRPM, res.idleVolCurve, res.idlePitchCurve, res.idle_lowVolCurve, res.idle_lowPitchCurve, res.lowVolCurve, res.lowPitchCurve, res.low_medVolCurve, res.low_medPitchCurve, res.medVolCurve, res.medPitchCurve, res.med_highVolCurve, res.med_highPitchCurve, res.highVolCurve, res.highPitchCurve, res.very_highVolCurve, res.very_highPitchCurve, res.maxRPMVolCurve);
                if (res.soundLevels == DynamicSoundController.SoundLevels.Three)
                    RES2Editor.EditCurves(RES2Editor.SoundLevels.Three, res.useRPMLimit, res.onLoadVolumeByRPM, res.offLoadVolumeByRPM, res.idleVolCurve, res.idlePitchCurve, res.idle_lowVolCurve, res.idle_lowPitchCurve, res.lowVolCurve, res.lowPitchCurve, res.low_medVolCurve, res.low_medPitchCurve, res.medVolCurve, res.medPitchCurve, res.med_highVolCurve, res.med_highPitchCurve, res.highVolCurve, res.highPitchCurve, res.very_highVolCurve, res.very_highPitchCurve, res.maxRPMVolCurve);
                if (res.soundLevels == DynamicSoundController.SoundLevels.One)
                    RES2Editor.EditCurves(RES2Editor.SoundLevels.One, res.useRPMLimit, res.onLoadVolumeByRPM, res.offLoadVolumeByRPM, res.idleVolCurve, res.idlePitchCurve, res.idle_lowVolCurve, res.idle_lowPitchCurve, res.lowVolCurve, res.lowPitchCurve, res.low_medVolCurve, res.low_medPitchCurve, res.medVolCurve, res.medPitchCurve, res.med_highVolCurve, res.med_highPitchCurve, res.highVolCurve, res.highPitchCurve, res.very_highVolCurve, res.very_highPitchCurve, res.maxRPMVolCurve);
            }
            if (editSettings)
            {
                // prefab general settings
                GUI.backgroundColor = new Color(.6f, .6f, .6f);
                GUI.backgroundColor = new Color(.7f, .7f, .7f);
                EditorGUILayout.BeginVertical(GUI.skin.button);
                EditorGUILayout.LabelField("", EditorStyles.boldLabel, GUILayout.Height(2));
                EditorGUILayout.LabelField("General Settings:", EditorStyles.boldLabel);
                RES2Editor.DrawUILine(new Color(.4f, .4f, .4f), 1, 1);
                GUI.backgroundColor = new Color(1f, 1f, 1f);
                res.masterVolume = EditorGUILayout.Slider(new GUIContent("Master Volume", "Sets the maximum volume an audio source can reach."), res.masterVolume, 0, 1);
                res.pitchMultiplier = EditorGUILayout.Slider(new GUIContent("Pitch Multiplier", "Multiplies audio sources pitch value by this value."), res.pitchMultiplier, 0.1f, 2f);
                res.idlePitchMultiplier = EditorGUILayout.Toggle(new GUIContent("Idle Pitch Multiplier", "Enable or disable Idle clip's pitch multiplication with 'Pitch multiplier' value."), res.idlePitchMultiplier);
                cameraType = (CameraType)EditorGUILayout.EnumPopup(new GUIContent("View Position: ", "Choose in which camera view you will use this prefab. This will switch between the Reversing SFX and Wind Noise SFX clips exterior / interior view sounds. Changing this value does not change the motor sound clips."), cameraType);
                if (cameraType == CameraType.Interior)
                    res.isInterior = true;
                else
                    res.isInterior = false;
                EditorGUI.BeginChangeCheck();
                prefabType = (PrefabType)EditorGUILayout.EnumPopup(new GUIContent("Audio Mixing Type: ", "Different audio mixer is used for player and for opponents. You can even use your own audio mixer by changing this to 'Custom'. Change between 'Player' and 'Oppoent' values when the vehicle is controller by the player or by an opponent. Do not use player type prefabs for opponents because it will conflict the sound mixing. This works like this because in Unity prefabs can't instatiate their own audio mixer. In an open word game you should change this value everytime when player enter/exit a vehicle."), prefabType);
                if (EditorGUI.EndChangeCheck())
                {
                    if (prefabType == PrefabType.Opponent || prefabType == PrefabType.Custom)
                    {
                        if (res.dynamicAudioMixer == RealisticEngineSound.DynamicAudioMixer.On)
                            _isDynamicMixer = true;
                        else
                            _isDynamicMixer = false;
                        res.dynamicAudioMixer = RealisticEngineSound.DynamicAudioMixer.Off;
                    }
                    if (prefabType == PrefabType.Player)
                    {
                        if (_isDynamicMixer)
                            res.dynamicAudioMixer = RealisticEngineSound.DynamicAudioMixer.On;
                        else
                            res.dynamicAudioMixer = RealisticEngineSound.DynamicAudioMixer.Off;
                    }
                }
                /*if (prefabType == PrefabType.Opponent || prefabType == PrefabType.Custom)
                    res.isOpponent = true;
                else
                    res.isOpponent = false;*/
                if (prefabType == PrefabType.Player)
                    res.prefabType = RealisticEngineSound.PrefabType.Player;
                if (prefabType == PrefabType.Opponent)
                    res.prefabType = RealisticEngineSound.PrefabType.Opponent;
                if (prefabType == PrefabType.Custom)
                    res.prefabType = RealisticEngineSound.PrefabType.Custom;
                GUI.backgroundColor = new Color(1f, 1f, 1f);
                res.useRPMLimit = EditorGUILayout.Toggle("Use Rev Limiter", res.useRPMLimit);
                if (res.useRPMLimit)
                    res.revLimiterTweaker = EditorGUILayout.Slider(new GUIContent("Rev Limiter Tweaker", "Sets the volume of very_highOn audio source while rev limiter is played. The higher the value the more it will reduce very_highOn audio volume."), res.revLimiterTweaker, 1f, 3f);
                res.maxRPMLimit = EditorGUILayout.FloatField("Maximum RPM", res.maxRPMLimit);
                res.gasPedalValueSetting = (RealisticEngineSound.GasPedalValue)EditorGUILayout.EnumPopup("Engine Load Value", res.gasPedalValueSetting);
                if (res.gasPedalValueSetting == RealisticEngineSound.GasPedalValue.Simulated)
                {
                    res.gasPedalSimSpeed = EditorGUILayout.Slider("Engine Load Simulation Speed", res.gasPedalSimSpeed, 1f, 15f);
                }
                else
                {
                    res.engineLoad = EditorGUILayout.Slider("Engine Load Value", res.engineLoad, 0.0f, 1.0f);
                }
                res.soundLevels = (DynamicSoundController.SoundLevels)EditorGUILayout.EnumPopup(new GUIContent("Engine Sound Levels", "Determine how much audio clips will be used for this prefab. It works like this: idle clip + sound levels + rev limiter + other optional sfx (like wind noise, reverse gear sfx, etc). If set to „seven” then prefab can use up to 16 audio clips + other sfx, if it is set to three then up to 8 audio clips will be used (the number of used audio clips is also depnding on „Off Load Type” value). „Level seven” has the following levels: idle-low, low, low-med, med, med-high, high and very-high. „Level three” has the following levels: low, med and very high."), res.soundLevels);
                res.offLoadType = (RealisticEngineSound.OffLoadType)EditorGUILayout.EnumPopup(new GUIContent("Offload Type", "If set to pre-recorded then it will use the pre-recorded audio clips for engine off load, with this setting the prefab will use more audio sources. If set to simulated then it will mimic engine off load sounds with On load audio clips by using audio mixing techiques, this will also reduce the used audio sources to almost half what it will be with pre-recorded off load sounds. These audio mixing tehniques will only work if the prefab usage is set to „Player” and the default audio mixer is used in the prefab. If set to „Opponent” then Off Load sound will be mimicked with the volume of On Load audio clips."), res.offLoadType);
                if (!res.isInterior && res.prefabType == RealisticEngineSound.PrefabType.Player)
                    res.dynamicAudioMixer = (RealisticEngineSound.DynamicAudioMixer)EditorGUILayout.EnumPopup(new GUIContent("Dynamic Audio Mixer", "This feature requires your scene's Audio Listener and the AudiMixer that is included with the asset. Dynamically EQ mixing the prefab's audio based on the player camera's current view. The car will sound differently when the player is looking at the front or at the back of the vehicle."), res.dynamicAudioMixer);
                if (res.dynamicAudioMixer == RealisticEngineSound.DynamicAudioMixer.On)
                {
                    res.highFreqMin = EditorGUILayout.Slider(new GUIContent("High Frequency Minimum", "This sets the minimum value for high frequencies when the camera is looking at the back of the player's car. This feature requires the AudiMixer that is included with the asset."), res.highFreqMin, 0.75f, 1.1f);
                    res.eqMedBoost = EditorGUILayout.Slider(new GUIContent("Mid Frequency Boost", "Original value is: 1.0f. It will 'boost' the prefab sound's mid frequency, this can be also used to 'de-boost' the prefab's mid frequencies. This feature requires the AudiMixer that is included with the asset. Also good for simulating stock / tuned car sounds in a racing game."), res.eqMedBoost, 0.5f, 1.5f);
                }
                EditorGUILayout.EndVertical();
                // audio source settings
                GUI.backgroundColor = new Color(.7f, .7f, .7f);
                EditorGUILayout.BeginVertical(GUI.skin.button);
                EditorGUILayout.LabelField("", EditorStyles.boldLabel, GUILayout.Height(2));
                EditorGUILayout.LabelField("Audio Source Settings:", EditorStyles.boldLabel);
                RES2Editor.DrawUILine(new Color(.4f, .4f, .4f), 1, 1);
                GUI.backgroundColor = new Color(1f, 1f, 1f);
                if (res.prefabType != RealisticEngineSound.PrefabType.Custom)
                {
                    res.audioMixer = EditorGUILayout.ObjectField(new GUIContent("Player Audio Mixer", "Used for dynamic audio mixing (in this case it is required to use that audimixer that is shipped with the asset) or you can use your own audio mixer if you are disabled dynamic audio mixing."), res.audioMixer, typeof(AudioMixerGroup), true) as AudioMixerGroup;
                    res.opponentAudioMixer = EditorGUILayout.ObjectField(new GUIContent("Opponent Audio Mixer", "Different audio mixer is used for opponent because those prefabs does not use dynamic audio mixing."), res.opponentAudioMixer, typeof(AudioMixerGroup), true) as AudioMixerGroup;
                }
                if (res.prefabType == RealisticEngineSound.PrefabType.Custom)
                    res.customAudioMixer = EditorGUILayout.ObjectField(new GUIContent("Custom Audio Mixer", "Can be used in casses when the pre-made audiomixer are not suitable for a project. If you want to use this, a custom audio mixer is need to be created by You."), res.customAudioMixer, typeof(AudioMixerGroup), true) as AudioMixerGroup;
                res.dopplerLevel = EditorGUILayout.Slider("Doppler Level", res.dopplerLevel, 0f, 5f);
                EditorGUI.BeginChangeCheck();
                res.minDistance = EditorGUILayout.FloatField(new GUIContent("Minimum Distance", "Within the MinDistance, the sound will stay at loudest possible. Outside MinDistance it will begin to attenuate. Increase the MinDistance of a sound to make it ‘louder’ in a 3d world, and decrease it to make it ‘quieter’ in a 3d world."), res.minDistance);
                res.maxDistance = EditorGUILayout.FloatField(new GUIContent("Maximum Distance", "The distance where the sound stops attenuating at."), res.maxDistance);
                res.audioRolloffMode = (AudioRolloffMode)EditorGUILayout.EnumPopup("Audio Rolloff Mode", res.audioRolloffMode);
                res.audioVelocityUpdateMode = (AudioVelocityUpdateMode)EditorGUILayout.EnumPopup(new GUIContent("Audio Velocity Update Mode", "Describes when an AudioSource or AudioListener is updated. Warning! Changing this value can cause strange behavior with car sounds. AUTO: Updates the source or listener in the FixedUpdate loop if it is attached to a Rigidbody, dynamic Update otherwise. FIXED: Updates the source or listener in the FixedUpdate loop. DYNAMIC: Updates the source or listener in the dynamic Update loop."), res.audioVelocityUpdateMode);
                if (EditorGUI.EndChangeCheck())
                    res.DestroyAll();
                res.reverbZoneSetting = (AudioReverbPreset)EditorGUILayout.EnumPopup("Audio Reverb Preset", res.reverbZoneSetting);
                GUI.backgroundColor = new Color(.75f, .75f, .75f);
                GUI.backgroundColor = new Color(1f, 1f, 1f);
                EditorGUI.BeginChangeCheck();
                res.optimiseAudioSources = (DynamicSoundController.OptimiseAudioSources)EditorGUILayout.EnumPopup(new GUIContent("Optimise Audio Sources", "Pause or destroy audio sources that are currently not needed or can't be heard because of their farther distance. If 'Pause' chosed, unused audio sources will be forcely paused. If 'Destroy' is choosed unused audio sources will be deleted. These deleted audio sources later will be re-created when they needed. Warning! 'Destroy' feature may generate some GC allocation, but it will help reducing the number of currently used Audio Sources in your scene."), res.optimiseAudioSources);
                if (EditorGUI.EndChangeCheck())
                    res.DestroyAll();
                if (res.optimiseAudioSources == DynamicSoundController.OptimiseAudioSources.PauseSlowly || res.optimiseAudioSources == DynamicSoundController.OptimiseAudioSources.PauseFastly || res.optimiseAudioSources == DynamicSoundController.OptimiseAudioSources.Destroy)
                    res.optimisationLevel = EditorGUILayout.Slider(new GUIContent("Optimisation Level", "Audio sources whose volume is currently less that this value will be stop playing or will be deleted. (depending on the above 'Optimisation' setting). Using too much value may produce audio 'popping', 'clipping' or 'laggy' like sounds."), res.optimisationLevel, 0.0f, 0.15f);
                res.audioListener = EditorGUILayout.ObjectField(new GUIContent("Audio Listener", "Audio Listener is required for optimising the audio usage. It will try to get the right audio listener automatically if it is not set by hand. The Audio Listener will helps to detect which audio sources should be paused or removed if they can't be heard of their farther distance."), res.audioListener, typeof(AudioListener), true) as AudioListener;
                EditorGUILayout.EndVertical();
                // reverse sound fx
                GUI.backgroundColor = new Color(.7f, .7f, .7f);
                EditorGUILayout.BeginVertical(GUI.skin.button);
                EditorGUILayout.LabelField("", EditorStyles.boldLabel, GUILayout.Height(2));
                EditorGUILayout.LabelField(new GUIContent("Reverse SoundFx Settings:", "Reverse gear's 'whining' soundfx settings."), EditorStyles.boldLabel);
                RES2Editor.DrawUILine(new Color(.4f, .4f, .4f), 1, 1);
                GUI.backgroundColor = new Color(1f, 1f, 1f);
                res.reverseGearSFX = (RealisticEngineSound.ReverseGearSFX)EditorGUILayout.EnumPopup(new GUIContent("Use Reverse Gear SFX", "Enable reverse gear's 'whining' soundfx and it’s settings"), res.reverseGearSFX);
                // bool
                if (!res.enableReverseGear && res.enableReverseGear != enableReverseGear)
                {
                    enableReverseGear = res.enableReverseGear;
                    res.reverseGearSFX = RealisticEngineSound.ReverseGearSFX.Off;
                    _reverseGearSFX = ReverseGearSFX.Off;
                }
                if (res.enableReverseGear && res.enableReverseGear != enableReverseGear)
                {
                    enableReverseGear = res.enableReverseGear;
                    res.reverseGearSFX = RealisticEngineSound.ReverseGearSFX.On;
                    _reverseGearSFX = ReverseGearSFX.On;
                }
                // enum
                if (res.reverseGearSFX == RealisticEngineSound.ReverseGearSFX.On && _reverseGearSFX == ReverseGearSFX.Off)
                {
                    res.enableReverseGear = true;
                    enableReverseGear = true;
                    _reverseGearSFX = ReverseGearSFX.On;
                }
                if (res.reverseGearSFX == RealisticEngineSound.ReverseGearSFX.Off && _reverseGearSFX == ReverseGearSFX.On)
                {
                    res.enableReverseGear = false;
                    enableReverseGear = false;
                    _reverseGearSFX = ReverseGearSFX.Off;
                }
                if (res.enableReverseGear)
                {
                    res.reversingVolCurve = EditorGUILayout.CurveField(new GUIContent("Reverse Gear SFX Volume Curve", "Set reverse gear sound effect's volume by the vehicles current speed."), res.reversingVolCurve, Color.white, new Rect(0, 0, 0, 0), GUILayout.Height(50));
                    res.reversingPitchCurve = EditorGUILayout.CurveField(new GUIContent("Reverse Gear SFX Pitch Curve", "Set reverse gear sound effect's pitch by the vehicles current speed."), res.reversingPitchCurve, Color.white, new Rect(0, 0, 0, 0), GUILayout.Height(50));
                    reverseGearClipType = (ReverseGearClipType)EditorGUILayout.EnumPopup(new GUIContent("Reverse Gear SFX Clips Type: ", "'Preset' sound clips are hard coded and can be choosen easly here only in editor mode (will not work in a built play mode) or set it to 'Custom' type to use your own sound clip."), reverseGearClipType);
                    EditorGUI.BeginChangeCheck();
                    if (reverseGearClipType == ReverseGearClipType.Preset)
                    {
                        res.customReverseClip = false;
                    }
                    else
                    {
                        res.customReverseClip = true;
                    }
                    if (EditorGUI.EndChangeCheck())
                        if (!res.customReverseClip)
                            res.reversingClip = RES2Editor.ChangeReverseClip(res.reverseClipID, res.isInterior, res.reversingClip);
                    if (!res.customReverseClip) // preset reverse clip
                    {
                        EditorGUILayout.BeginHorizontal();
                        // choosen reverse noise clip text
                        // customizing font
                        changeTextSizeGuiStyle.fontStyle = FontStyle.Normal;
                        changeTextSizeGuiStyle.alignment = TextAnchor.LowerLeft;
                        GUILayout.Label("", GUILayout.Width(12), GUILayout.Height(15));
                        GUILayout.Label(new GUIContent("Choosen clip: "), changeTextSizeGuiStyle, GUILayout.Width(85), GUILayout.Height(18));
                        changeTextSizeGuiStyle.fontStyle = FontStyle.BoldAndItalic;
                        if (!res.isInterior)
                            GUILayout.Label("reversing_" + res.reverseClipID + ".wav", changeTextSizeGuiStyle, GUILayout.Height(18));
                        else
                            GUILayout.Label("int_reversing_" + res.reverseClipID + ".wav", changeTextSizeGuiStyle, GUILayout.Height(18));
                        // show file missing error
                        if (EditorGUIUtility.Load(assetPath + "/Assets/Sounds/Reversing/reversing_" + res.reverseClipID + ".wav") == null || EditorGUIUtility.Load(assetPath + "/Assets/Sounds/Reversing/Interior/int_reversing_" + res.reverseClipID + ".wav") == null)
                        {
                            redTextSizeGuiStyle.normal.textColor = Color.red;
                            redTextSizeGuiStyle.alignment = TextAnchor.LowerLeft;
                            GUILayout.Label("file missing!", redTextSizeGuiStyle, GUILayout.Width(55), GUILayout.Height(18));
                        }
                        // button previous
                        if (res.reverseClipID < 1)
                            res.reverseClipID = 1;
                        GUILayout.Label(""); // space
                        EditorGUI.BeginDisabledGroup(res.reverseClipID == 1);
                        if (GUILayout.Button("Previous", GUILayout.Width(94)))
                        {
                            res.reverseClipID -= 1;
                            res.reversingClip = RES2Editor.ChangeReverseClip(res.reverseClipID, res.isInterior, res.reversingClip);
                        }
                        EditorGUI.EndDisabledGroup();
                        // button next
                        if (res.reverseClipID > 5)
                            res.reverseClipID = 5;
                        EditorGUI.BeginDisabledGroup(res.reverseClipID == 5);
                        if (GUILayout.Button("Next", GUILayout.Width(94)))
                        {
                            res.reverseClipID += 1;
                            if (res.reverseClipID != 6)
                                res.reversingClip = RES2Editor.ChangeReverseClip(res.reverseClipID, res.isInterior, res.reversingClip);
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Label("", GUILayout.Height(4));
                    }
                    else// custom reverse gear fx clip
                    {
                        EditorGUILayout.BeginHorizontal();
                        res.reversingClip = EditorGUILayout.ObjectField(new GUIContent("Custom Reverse Gear SFX Clip"), res.reversingClip, typeof(AudioClip), true) as AudioClip;
                        EditorGUILayout.EndHorizontal();
                    }
                }
                RES2Editor.DrawUILine(new Color(.4f, .4f, .4f), 1, 1);
                // reverse beep
                res.reverseBeepSFX = (RealisticEngineSound.ReverseBeepSFX)EditorGUILayout.EnumPopup(new GUIContent("Use Reverse Beep SFX", "Reverse gear's beep warning sound which will be played in reverse gear."), res.reverseBeepSFX);
                // bool
                if (!res.enableReverseBeep && res.enableReverseBeep != enableReverseBeep)
                {
                    enableReverseBeep = res.enableReverseBeep;
                    res.reverseBeepSFX = RealisticEngineSound.ReverseBeepSFX.Off;
                    _reverseBeepSFX = ReverseBeepSFX.Off;
                }
                if (res.enableReverseBeep && res.enableReverseBeep != enableReverseBeep)
                {
                    enableReverseBeep = res.enableReverseBeep;
                    res.reverseBeepSFX = RealisticEngineSound.ReverseBeepSFX.On;
                    _reverseBeepSFX = ReverseBeepSFX.On;
                }
                // enum
                if (res.reverseBeepSFX == RealisticEngineSound.ReverseBeepSFX.On && _reverseBeepSFX == ReverseBeepSFX.Off)
                {
                    res.enableReverseBeep = true;
                    enableReverseBeep = true;
                    _reverseBeepSFX = ReverseBeepSFX.On;
                }
                if (res.reverseBeepSFX == RealisticEngineSound.ReverseBeepSFX.Off && _reverseBeepSFX == ReverseBeepSFX.On)
                {
                    res.enableReverseBeep = false;
                    enableReverseBeep = false;
                    _reverseBeepSFX = ReverseBeepSFX.Off;
                }
                if (res.enableReverseBeep)
                {
                    res.reverseBeepMaster = EditorGUILayout.Slider(new GUIContent("Reverse Beep Master Volume", "The master volume of reverse gear's beeping warning soundfx."), res.reverseBeepMaster, 0.1f, 1);
                    reverseBeepClipType = (ReverseBeepClipType)EditorGUILayout.EnumPopup(new GUIContent("Reverse Beep SFX Clips Type: ", "'Preset' sound clips are hard coded and can be choosen easly here only in editor mode (will not work in a built play mode) or set it to 'Custom' type to use your own sound clip."), reverseBeepClipType);
                    EditorGUI.BeginChangeCheck();
                    if (reverseBeepClipType == ReverseBeepClipType.Preset)
                    {
                        res.customReverseBeepClip = false;
                    }
                    if (reverseBeepClipType == ReverseBeepClipType.Custom)
                    {
                        res.customReverseBeepClip = true;
                    }
                    if (EditorGUI.EndChangeCheck())
                        if (!res.customReverseBeepClip)
                            res.reversingBeepClip = RES2Editor.ChangeReverseBeepClip(res.reverseBeepClipID, res.isInterior, res.reversingBeepClip);
                    if (!res.customReverseBeepClip) // preset reverse clip
                    {
                        EditorGUILayout.BeginHorizontal();
                        // choosen reverse noise clip text
                        // customizing font
                        changeTextSizeGuiStyle.fontStyle = FontStyle.Normal;
                        changeTextSizeGuiStyle.alignment = TextAnchor.LowerLeft;
                        GUILayout.Label("", GUILayout.Width(12), GUILayout.Height(15));
                        GUILayout.Label(new GUIContent("Choosen clip: "), changeTextSizeGuiStyle, GUILayout.Width(85), GUILayout.Height(18));
                        changeTextSizeGuiStyle.fontStyle = FontStyle.BoldAndItalic;
                        if (!res.isInterior)
                            GUILayout.Label("beep_" + res.reverseBeepClipID + ".wav", changeTextSizeGuiStyle, GUILayout.Height(18));
                        else
                            GUILayout.Label("int_beep_" + res.reverseBeepClipID + ".wav", changeTextSizeGuiStyle, GUILayout.Height(18));
                        // show file missing error
                        if (EditorGUIUtility.Load(assetPath + "/Assets/Sounds/Reversing/beep_" + res.reverseBeepClipID + ".wav") == null || EditorGUIUtility.Load(assetPath + "/Assets/Sounds/Reversing/Interior/int_beep_" + res.reverseBeepClipID + ".wav") == null)
                        {
                            redTextSizeGuiStyle.normal.textColor = Color.red;
                            redTextSizeGuiStyle.alignment = TextAnchor.LowerLeft;
                            GUILayout.Label("file missing!", redTextSizeGuiStyle, GUILayout.Width(55), GUILayout.Height(18));
                        }
                        // button previous
                        if (res.reverseBeepClipID < 1)
                            res.reverseBeepClipID = 1;
                        GUILayout.Label(""); // space
                        EditorGUI.BeginDisabledGroup(res.reverseBeepClipID == 1);
                        if (GUILayout.Button("Previous", GUILayout.Width(94)))
                        {
                            res.reverseBeepClipID -= 1;
                            res.reversingBeepClip = RES2Editor.ChangeReverseBeepClip(res.reverseBeepClipID, res.isInterior, res.reversingBeepClip);
                        }
                        EditorGUI.EndDisabledGroup();
                        // button next
                        if (res.reverseBeepClipID > 4)
                            res.reverseBeepClipID = 4;
                        EditorGUI.BeginDisabledGroup(res.reverseBeepClipID == 4);
                        if (GUILayout.Button("Next", GUILayout.Width(94)))
                        {
                            res.reverseBeepClipID += 1;
                            if (res.reverseBeepClipID != 5)
                                res.reversingBeepClip = RES2Editor.ChangeReverseBeepClip(res.reverseBeepClipID, res.isInterior, res.reversingBeepClip);
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Label("", GUILayout.Height(4));
                    }
                    else// custom reverse gear fx clip
                    {
                        EditorGUILayout.BeginHorizontal();
                        res.reversingBeepClip = EditorGUILayout.ObjectField(new GUIContent("Custom Reverse Gear SFX Clip"), res.reversingBeepClip, typeof(AudioClip), true) as AudioClip;
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();
                // wind noise settings
                GUI.backgroundColor = new Color(.7f, .7f, .7f);
                EditorGUILayout.BeginVertical(GUI.skin.button);
                EditorGUILayout.LabelField("", EditorStyles.boldLabel, GUILayout.Height(2));
                EditorGUILayout.LabelField(new GUIContent("Wind Noise Settings:", "Edit wind noise's audio settings. You can edit the Wind Noise Sound's volume and pitch values in 'Edit Curves' tab by clicking on it's curve editor. Preset audio clip changing buttons only work in Unity Editor, it will not work in a built game because the editor script is not building with the game."), EditorStyles.boldLabel);
                RES2Editor.DrawUILine(new Color(.4f, .4f, .4f), 1, 1);
                GUI.backgroundColor = new Color(1f, 1f, 1f);
                res.windNoiseEnabled = (DynamicSoundController.WindNoiseEnum)EditorGUILayout.EnumPopup(new GUIContent("Enable Wind Noise", "Enable wind noise and it’s audio settings. Enabling wind noise will add a wind noise sound effect that is affected by your vehicle’s current speed. It will add a little detail to your vehicle's sound."), res.windNoiseEnabled);
                if (res.windNoiseEnabled == DynamicSoundController.WindNoiseEnum.On)
                {
                    res.windMasterVolume = EditorGUILayout.Slider(new GUIContent("Wind Noise Master Volume", "The master volume of the wind noise within this prefab."), res.windMasterVolume, 0, 1);
                    res.windMinDistance = EditorGUILayout.FloatField(new GUIContent("Minimum Distance", "Within the Min distance the wind noise will cease to grow louder in volume."), res.windMinDistance);
                    res.windMaxDistance = EditorGUILayout.FloatField(new GUIContent("Maximum Distance", "(Logarithmic rolloff) MaxDistance is the distance a sound stops attenuating at."), res.windMaxDistance);
                    res.windPosition = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Wind Noise SFX Position (Optional)", "Wind noise sfx will be instantiated in this gameobject. Position this gameobject somewhere in your vehicle if you want to change the wind noise's position. If you leave it empty, Wind Noise will be instantiated at engine sound audios position."), res.windPosition, typeof(GameObject), true);
                    res.windMixer = EditorGUILayout.ObjectField(new GUIContent("Wind Audio Mixer (Optional)", "You can use a separate audio mixer for wind noises if you want to. Otherwise it will use the engine sound's audio mixer if any is used."), res.windMixer, typeof(AudioMixerGroup), true) as AudioMixerGroup;
                    EditorGUILayout.LabelField("", GUILayout.Height(2));
                    windClipType = (WindClipType)EditorGUILayout.EnumPopup(new GUIContent("Wind Noise Clips Type: ", "'Preset' sound clips are hard coded and can be choosen easly here only in editor mode (will not work in a built play mode) or set it to 'Custom' type to use your own sound clip."), windClipType);
                    EditorGUI.BeginChangeCheck();
                    if (windClipType == WindClipType.Preset)
                    {
                        res.customWindClip = false;
                    }
                    else
                    {
                        res.customWindClip = true;
                    }
                    if (EditorGUI.EndChangeCheck())
                        if (!res.customWindClip)
                            res.windNoiseClip = RES2Editor.ChangeWindClip(res.windClipID, res.isInterior, res.windNoiseClip);
                    if (!res.customWindClip) // preset wind clip
                    {
                        EditorGUILayout.BeginHorizontal();
                        // choosen wind noise clip text
                        // customizing font
                        changeTextSizeGuiStyle.fontStyle = FontStyle.Normal;
                        changeTextSizeGuiStyle.alignment = TextAnchor.LowerLeft;
                        GUILayout.Label("", GUILayout.Width(12), GUILayout.Height(15));
                        GUILayout.Label(new GUIContent("Choosen clip: "), changeTextSizeGuiStyle, GUILayout.Width(85), GUILayout.Height(18));
                        changeTextSizeGuiStyle.fontStyle = FontStyle.BoldAndItalic;
                        if (!res.isInterior)
                            GUILayout.Label("wind_" + res.windClipID + ".wav", changeTextSizeGuiStyle, GUILayout.Height(18));
                        else
                            GUILayout.Label("int_wind_" + res.windClipID + ".wav", changeTextSizeGuiStyle, GUILayout.Height(18));
                        // show file missing error
                        if (EditorGUIUtility.Load(assetPath + "/Assets/Sounds/Wind_Sounds/wind_" + res.windClipID + ".wav") == null || EditorGUIUtility.Load(assetPath + "/Assets/Sounds/Wind_Sounds/Interior/int_wind_" + res.windClipID + ".wav") == null)
                        {
                            redTextSizeGuiStyle.normal.textColor = Color.red;
                            redTextSizeGuiStyle.alignment = TextAnchor.LowerLeft;
                            GUILayout.Label("file missing!", redTextSizeGuiStyle, GUILayout.Width(55), GUILayout.Height(18));
                        }
                        // button previous
                        if (res.windClipID < 1)
                            res.windClipID = 1;
                        GUILayout.Label(""); // space
                        EditorGUI.BeginDisabledGroup(res.windClipID == 1);
                        if (GUILayout.Button("Previous", GUILayout.Width(94)))
                        {
                            res.windClipID -= 1;
                            res.windNoiseClip = RES2Editor.ChangeWindClip(res.windClipID, res.isInterior, res.windNoiseClip);
                        }
                        EditorGUI.EndDisabledGroup();
                        // button next
                        if (res.windClipID > 6)
                            res.windClipID = 6;
                        EditorGUI.BeginDisabledGroup(res.windClipID == 6);
                        if (GUILayout.Button("Next", GUILayout.Width(94)))
                        {
                            res.windClipID += 1;
                            if (res.windClipID != 7)
                                res.windNoiseClip = RES2Editor.ChangeWindClip(res.windClipID, res.isInterior, res.windNoiseClip);
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Label("", GUILayout.Height(4));
                    }
                    else// custom wind noise clip
                    {
                        EditorGUILayout.BeginHorizontal();
                        res.windNoiseClip = EditorGUILayout.ObjectField(new GUIContent("Custom Wind noise clip"), res.windNoiseClip, typeof(AudioClip), true) as AudioClip;
                        EditorGUILayout.EndHorizontal();
                    }
                    res.volumeCurveWind = EditorGUILayout.CurveField(new GUIContent("Wind Noise Volume Curve", "Set wind noise sound effect's volume affected by the vehicles current speed."), res.volumeCurveWind, Color.white, new Rect(0, 0, 0, 0), GUILayout.Height(50));
                    res.pitchCurveWind = EditorGUILayout.CurveField(new GUIContent("Wind Noise Pitch Curve", "Set wind noise sound effect's pitch affected by the vehicles current speed."), res.pitchCurveWind, Color.white, new Rect(0, 0, 0, 0), GUILayout.Height(50));
                }
                EditorGUILayout.EndVertical();
                // engine shake fx settings
                if (res.gasPedalValueSetting == RealisticEngineSound.GasPedalValue.Simulated)
                {
                    EditShakeFX();
                }
                // valve fx settings
                GUI.backgroundColor = new Color(.7f, .7f, .7f);
                EditorGUILayout.BeginVertical(GUI.skin.button);
                EditorGUILayout.LabelField("", EditorStyles.boldLabel, GUILayout.Height(2));
                EditorGUILayout.LabelField(new GUIContent("Valves SFX Settings:", "Enabling this feature will geneare some GC (garbage) for a short time. Valve sound effect add some additional sound effect that simulate valves clicking sound as frequently as much rpm have the vehicle currently. For realistic results, this effect should be positioned to the vehicle's engine position."), EditorStyles.boldLabel);
                RES2Editor.DrawUILine(new Color(.4f, .4f, .4f), 1, 1);
                GUI.backgroundColor = new Color(1f, 1f, 1f);
                res.enableValves = (DynamicSoundController.EngineValve)EditorGUILayout.EnumPopup(new GUIContent("Enable Valves SFX", "Valve sound fx will be played as frequently as much rpm has the prefab currently. This sfx is good for a game where you can open the engine hood / bonet, this can simulate those white noises that a running engine has inside the enginebay."), res.enableValves);
                if (res.enableValves == DynamicSoundController.EngineValve.On)
                {
                    res.valvePosition = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Valve Sound FX Position (Optional)", "Valve sound fx will be instantiated in this gameobject. Position this gameobject into your vehicles engine bay for more realistic effect. If you leave it empty, Valve FX will be instantiated at engine sound audios position."), res.valvePosition, typeof(GameObject), true);
                    SerializedProperty arrayProp = serializedObject.FindProperty("valveClips");
                    // array start
                    arrayProp.arraySize = EditorGUILayout.IntField("Valve Audio Clips:", arrayProp.arraySize);
                    for (int i = 0; i < arrayProp.arraySize; ++i)
                    {
                        SerializedProperty transformProp = arrayProp.GetArrayElementAtIndex(i);
                        EditorGUILayout.PropertyField(transformProp, new GUIContent("Element " + i));
                    }
                    // array end
                    serializedObject.ApplyModifiedProperties();
                    res.valvesMinDistance = EditorGUILayout.FloatField(new GUIContent("Valves Audio Minimum Distance", "Within the MinDistance, the sound will stay at loudest possible. Outside MinDistance it will begin to attenuate. Increase the MinDistance of a sound to make it ‘louder’ in a 3d world, and decrease it to make it ‘quieter’ in a 3d world."), res.valvesMinDistance);
                    res.valvesMaxDistance = EditorGUILayout.FloatField(new GUIContent("Valves Audio Maximum Distance", "The distance where the sound stops attenuating at."), res.valvesMaxDistance);
                    res.allValvesVolume = EditorGUILayout.Slider(new GUIContent("Valve Master Volume", "Sets the maximum volume of all valve sound fx shots."), res.allValvesVolume, 0f, 1f);
                    res.valvesPitch = EditorGUILayout.Slider(new GUIContent("Valves Pitch", "Sets the pitch of all valve sound fx shots."), res.valvesPitch, 0.5f, 2f);
                    res.valveSpeed = EditorGUILayout.Slider(new GUIContent("Valves Rate", "Sets the how frequently will be played one shot of a valve sound fx based on engine RPM."), res.valveSpeed, 0.0005f, 0.0025f);
                    res.valvesRPMVolume = EditorGUILayout.CurveField(new GUIContent("Valves volume by RPM", "Set the valve sound effects volume by the vehicles engine RPM."), res.valvesRPMVolume, Color.white, new Rect(0, 0, 0, 0), GUILayout.Height(50));
                }
                EditorGUILayout.EndVertical();
                // aggressivness fx
                GUI.backgroundColor = new Color(.7f, .7f, .7f);
                EditorGUILayout.BeginVertical(GUI.skin.button);
                EditorGUILayout.LabelField("", EditorStyles.boldLabel, GUILayout.Height(2));
                EditorGUILayout.LabelField(new GUIContent("Aggressiveness SFX Settings:", "Aggressiveness sound effect will add an aggressive noise to the engine sound."), EditorStyles.boldLabel);
                RES2Editor.DrawUILine(new Color(.4f, .4f, .4f), 1, 1);
                GUI.backgroundColor = new Color(1f, 1f, 1f);
                res.enableAggressivness = (RealisticEngineSound.AggressivnessEnum)EditorGUILayout.EnumPopup(new GUIContent("Enable Aggressiveness SFX", "Aggressiveness sound effect will add an aggressive noise to the engine sound."), res.enableAggressivness);
                if (res.enableAggressivness == RealisticEngineSound.AggressivnessEnum.On)
                {
                    res.aggressivnessOnClip = EditorGUILayout.ObjectField("Aggressiveness On Load Clip", res.aggressivnessOnClip, typeof(AudioClip), true) as AudioClip;
                    if (res.offLoadType == RealisticEngineSound.OffLoadType.Prerecorded)
                        res.aggressivnessOffClip = EditorGUILayout.ObjectField("Aggressiveness Off Load Clip", res.aggressivnessOffClip, typeof(AudioClip), true) as AudioClip;
                    else
                        EditorGUILayout.LabelField(new GUIContent("Off load sounds type are set to simulated.", "Under Settings tab in General Settings menu, Offload Type is set to Simulated which means all off load sounds are simulated instead of using pre-recorded offload sounds. Read more about this feature by pointing your cursor on 'Offload Type' text under General Settings menu, or read about it in the Documentation."), EditorStyles.boldLabel);
                    res.aggressivnessMaster = EditorGUILayout.Slider(new GUIContent("Aggressiveness Level", "Sets the level of aggressiveness sound effect."), res.aggressivnessMaster, 0f, 1f);
                    if (res.useRPMLimit)
                        res.revLimiterAggressTweaker = EditorGUILayout.Slider(new GUIContent("Rev Limiter Tweaker", "Sets the volume of aggressiveness audio source while rev limiter is played. The higher the value the more it will reduce aggressiveness audio volume."), res.revLimiterAggressTweaker, 1f, 3f);
                    res.aggressivnessVolCurve = EditorGUILayout.CurveField(new GUIContent("Aggressiveness Volume by RPM", "Set aggressiveness sound effect's volume by the vehicle's engine RPM."), res.aggressivnessVolCurve, Color.white, new Rect(0, 0, 0, 0), GUILayout.Height(50));
                    res.aggressivnessPitchCurve = EditorGUILayout.CurveField(new GUIContent("Aggressiveness Pitch by RPM", "Set Aggressiveness sound effect's pitch by the vehicle's engine RPM."), res.aggressivnessPitchCurve, Color.white, new Rect(0, 0, 0, 0), GUILayout.Height(50));
                }
                EditorGUILayout.EndVertical();
                GUI.backgroundColor = new Color(.7f, .7f, .7f);
                EditorGUILayout.BeginVertical(GUI.skin.button);
                EditorGUILayout.LabelField("", EditorStyles.boldLabel, GUILayout.Height(2));
                EditorGUILayout.LabelField(new GUIContent("Other Settings:", "Other, custom RES inspector related settings."), EditorStyles.boldLabel);
                RES2Editor.DrawUILine(new Color(.4f, .4f, .4f), 1, 1);
                GUI.backgroundColor = new Color(1f, 1f, 1f);
                debugInfo = EditorGUILayout.Toggle(new GUIContent("Debug Info", "Show detailed debug values in the inspector."), debugInfo);
                enableNotify = EditorGUILayout.Toggle(new GUIContent("Show Welcome Information", "Will show the welcome information in the inspector. This can be turned off and it is saved globally for all Unity projects."), enableNotify);
                EditorGUILayout.EndVertical();
            }
            GUI.backgroundColor = new Color(.7f, .7f, .7f);
            if (res.curvesByVolume == RealisticEngineSound.CurvesByVolume.ShowRawCurves)
            {
                if (res.showCurvesVolumeRPM)
                    res.showCurvesVolumeRPM = false;
                if (res.showCurvesOffVolumeRPM)
                    res.showCurvesOffVolumeRPM = false;
            }
            if (res.curvesByVolume == RealisticEngineSound.CurvesByVolume.ShowWithOffLoadVolume)
            {
                if (res.showCurvesVolumeRPM)
                    res.showCurvesVolumeRPM = false;
                if (!res.showCurvesOffVolumeRPM)
                    res.showCurvesOffVolumeRPM = true;
            }
            if (res.curvesByVolume == RealisticEngineSound.CurvesByVolume.ShowWithOnLoadVolume)
            {
                if (!res.showCurvesVolumeRPM)
                    res.showCurvesVolumeRPM = true;
                if (res.showCurvesOffVolumeRPM)
                    res.showCurvesOffVolumeRPM = false;
            }
            // undo
            if (!Application.isPlaying) // do not run this code in play mode
            {
                if (GUI.changed)
                {
                    EditorUtility.SetDirty(res);
                    EditorSceneManager.MarkSceneDirty(res.gameObject.scene);
                }
            }
        }
        void LoadAudioClips()
        {
            var res = target as RealisticEngineSound;
            var loadedAudio = RES2Editor.LoadAudioClips(res.prefabName, isInterior, res.idleClip, res.idle_lowOnClip, res.idle_lowOffClip, res.lowOnClip, res.lowOffClip, res.low_medOnClip, res.low_medOffClip, res.medOnClip, res.medOffClip, res.med_highOnClip, res.med_highOffClip, res.highOnClip, res.highOffClip, res.very_highOnClip, res.very_highOffClip, res.maxRPMClip, res.aggressivnessOnClip, res.aggressivnessOffClip, res.parentFolder);
            res.idleClip = loadedAudio.idleClip;
            res.idle_lowOnClip = loadedAudio.idle_lowOnClip;
            res.idle_lowOffClip = loadedAudio.idle_lowOffClip;
            res.lowOnClip = loadedAudio.lowOnClip;
            res.lowOffClip = loadedAudio.lowOffClip;
            res.low_medOnClip = loadedAudio.low_medOnClip;
            res.low_medOffClip = loadedAudio.low_medOffClip;
            res.medOnClip = loadedAudio.medOnClip;
            res.medOffClip = loadedAudio.medOffClip;
            res.med_highOnClip = loadedAudio.med_highOnClip;
            res.med_highOffClip = loadedAudio.med_highOffClip;
            res.highOnClip = loadedAudio.highOnClip;
            res.highOffClip = loadedAudio.highOffClip;
            res.very_highOnClip = loadedAudio.very_highOnClip;
            res.very_highOffClip = loadedAudio.very_highOffClip;
            res.maxRPMClip = loadedAudio.maxRPMClip;
            res.aggressivnessOnClip = loadedAudio.aggressivnessOnClip;
            res.aggressivnessOffClip = loadedAudio.aggressivnessOffClip;
        }
        void EditAudioClips(RES2Editor.SoundLevels soundLevels/*bool isSeven*/, bool isOffloadSimulated)
        {
            var res = target as RealisticEngineSound;
            var editClips = RES2Editor.EditClips(soundLevels/*isSeven*/, isOffloadSimulated, res.idleClip, res.idle_lowOnClip, res.idle_lowOffClip, res.lowOnClip, res.lowOffClip, res.low_medOnClip, res.low_medOffClip, res.medOnClip, res.medOffClip, res.med_highOnClip, res.med_highOffClip, res.highOnClip, res.highOffClip, res.very_highOnClip, res.very_highOffClip, res.maxRPMClip);
            res.idleClip = editClips.idleClip;
            res.idle_lowOnClip = editClips.idle_lowOnClip;
            res.idle_lowOffClip = editClips.idle_lowOffClip;
            res.lowOnClip = editClips.lowOnClip;
            res.lowOffClip = editClips.lowOffClip;
            res.low_medOnClip = editClips.low_medOnClip;
            res.low_medOffClip = editClips.low_medOffClip;
            res.medOnClip = editClips.medOnClip;
            res.medOffClip = editClips.medOffClip;
            res.med_highOnClip = editClips.med_highOnClip;
            res.med_highOffClip = editClips.med_highOffClip;
            res.highOnClip = editClips.highOnClip;
            res.highOffClip = editClips.highOffClip;
            res.very_highOnClip = editClips.very_highOnClip;
            res.very_highOffClip = editClips.very_highOffClip;
            res.maxRPMClip = editClips.maxRPMClip;
        }
        void EditShakeFX()
        {
            var res = target as RealisticEngineSound;
            var shakeSFXUI = RES2Editor.ShakeSFX(res.engineShakeSetting, res.shakeRateSetting, res.shakeLengthSetting, res.shakeVolumeChange, res.randomChance, res.shakeRate, res.shakeLength, res.shakeCarMinSpeed, res.shakeCarMinRPM);
            res.engineShakeSetting = shakeSFXUI.engineShakeSetting;
            res.shakeRateSetting = shakeSFXUI.shakeRateSetting;
            res.shakeLengthSetting = shakeSFXUI.shakeLengthSetting;
            res.shakeVolumeChange = shakeSFXUI.shakeVolumeChange;
            res.randomChance = shakeSFXUI.randomChance;
            res.shakeRate = shakeSFXUI.shakeRate;
            res.shakeLength = shakeSFXUI.shakeLength;
            res.shakeCarMinSpeed = shakeSFXUI.shakeCarMinSpeed;
            res.shakeCarMinRPM = shakeSFXUI.shakeCarMinRPM;
        }
        static void SaveEditorSettings()
        {
            // save
            EditorPrefs.SetBool("enableNotify", enableNotify);
            _enableNotify = enableNotify;
            EditorPrefs.SetBool("editorDebug", debugInfo);
            _debugInfo = debugInfo;
        }
        static void SaveInspectorState()
        {
            // save
            EditorPrefs.SetInt("inspectorPreviousState", inspectorPreviousState);
        }
        static void SaveCurveWindowState()
        {
            // save
            EditorPrefs.SetBool("curvesWindowState", showCurvesWindow);
        }
    }
}
