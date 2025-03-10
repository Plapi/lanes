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
using UnityEngine.UI;
using SkrilStudio;

namespace SkrilStudio
{
    public class DemoController : MonoBehaviour // Main demo scene script, only used for demonstration purposes and this script is not designed to be used in live products. This scipt is only designed to work only in the soundpack tester demo scenes, it will not work in other scenes and this script is not the main part of the asset, it can be deleted if you do not want to test the soundpacks anymore. Import the right *.unitypackage for your car controller or contact me in email for adding support for your custom car controller.
    {
        [Header("This script is only made for main DEMO scene")]
        public RealisticEngineSound[] allChildren;
        public GameObject gasPedalButton; // UI button
        public Slider rpmSlider; // UI slider to set RPM
        public Slider pitchSlider; // UI sliter to set maximum pitch
        public Slider aggressivenessSlider;
        public Slider currentSpeedSlider;
        public Text pitchText; // UI text to show pitch multiplier value
        public Text rpmText; // UI text to show current RPM
        public Text currentSpeed;
        public Toggle gasPedalPressing;
        public GameObject accelerationSpeed; // UI slider for acceleration speed
        public Image[] carSoundButtons;
        // wind noise & reversing sfx
        public Toggle hasReverseGearSound;
        public Toggle hasReverseBeepSound;
        public Toggle hasWindSound;
        public Button[] nextPreviousButtons; // 0 = wind previous button, 1 = wind next button, 2 = reversing gear previous button, 3 = reversing gear next button, 4 = reversing beep previous button, 5 = reversing beep next button
        public Text[] soundEffectsTexts; // 0 = wind noise selection text, 1 reversing gear sfx selection text, 2 reversing beep sfx selection text
        public AudioClip[] windClips;
        private int windID = 1; // set wind clip id
        public AudioClip[] reverseClips;
        private int reverseID = 1; // set wind clip id
        public AudioClip[] reverseBeepClips;
        private int reverseBeepID = 1; // set wind clip id
        //
        public bool simulated = true; // is rpm simulated with gaspedal button or with rpm slider by hand
        private int currentSoundpackID = 2; // raw value for current soundpack id
        private int currentSoundpakcIDandCam = 2; // id for exterior / interior soundpack
        private bool isInterior = false;
        CarSimulator carSimulator;
        private void Start()
        {
            allChildren = GetComponentsInChildren<RealisticEngineSound>(true);
            ChangeCarSound(currentSoundpakcIDandCam);
            carSimulator = gasPedalButton.GetComponent<CarSimulator>();
            // turn off all interior prefabs
            for (int i = 0; i < allChildren.Length; i++)
            {
                if (i % 2 == 0)
                    allChildren[i + 1].gameObject.SetActive(false);
            }
            // set values
            for (int i = 0; i < allChildren.Length; i++)
            {
                if (allChildren[i].GetComponent<RealisticEngineSound>() != null)
                    allChildren[i].GetComponent<RealisticEngineSound>().carMaxSpeed = 250;//7000;
            }
            // change buttons color
            carSoundButtons[currentSoundpackID / 2].color = Color.green; // set the choosed soundpack button's color to green
        }

        private void Update()
        {
            rpmText.text = "Engine RPM is at: " + (int)rpmSlider.value / 70 + "%"; // show current RPM - this creates garbage
            pitchText.text = "" + pitchSlider.value; // set pitch multiplier value for ui text
            currentSpeed.text = "" + currentSpeedSlider.value;
            // rpm values
            if (allChildren[currentSoundpakcIDandCam].GetComponent<RealisticEngineSound>() != null)
                allChildren[currentSoundpakcIDandCam].GetComponent<RealisticEngineSound>().pitchMultiplier = pitchSlider.value;
            if (allChildren[currentSoundpakcIDandCam].GetComponent<RealisticEngineSound>() != null)
                allChildren[currentSoundpakcIDandCam].GetComponent<RealisticEngineSound>().aggressivnessMaster = aggressivenessSlider.value;
            if (simulated)
            {
                if (allChildren[currentSoundpakcIDandCam].GetComponent<RealisticEngineSound>() != null)
                    allChildren[currentSoundpakcIDandCam].GetComponent<RealisticEngineSound>().engineCurrentRPM = carSimulator.rpm;
                rpmSlider.value = carSimulator.rpm; // set ui sliders value to rpm
            }
            else
            {
                if (allChildren[currentSoundpakcIDandCam].GetComponent<RealisticEngineSound>() != null)
                    allChildren[currentSoundpakcIDandCam].GetComponent<RealisticEngineSound>().engineCurrentRPM = rpmSlider.value;
                carSimulator.rpm = rpmSlider.value;
            }
            if (allChildren[currentSoundpakcIDandCam].GetComponent<RealisticEngineSound>() != null)
                allChildren[currentSoundpakcIDandCam].GetComponent<RealisticEngineSound>().carCurrentSpeed = currentSpeedSlider.value; // for reverse gear and wind noise sfx
            if (simulated) // update is gas pedal pressing toggle
                if (gasPedalPressing != null)
                    gasPedalPressing.isOn = carSimulator.gasPedalPressing;
        }

        // enable/disable rev limiter
        public void UpdateRPM(Toggle togl)
        {
            for (int i = 0; i < allChildren.Length; i++)
            {
                if (allChildren[i].GetComponent<RealisticEngineSound>() != null)
                    allChildren[i].GetComponent<RealisticEngineSound>().useRPMLimit = togl.isOn;
            }
        }
        // is reversing
        public void IsReversing(Toggle isReversingTgl)
        {
            for (int i = 0; i < allChildren.Length; i++)
            {
                if (allChildren[i].GetComponent<RealisticEngineSound>() != null)
                    allChildren[i].GetComponent<RealisticEngineSound>().isReversing = isReversingTgl.isOn;
            }
            // reversing gear sfx
            hasReverseGearSound.interactable = isReversingTgl.isOn;
            if (hasReverseGearSound.isOn)
            {
                nextPreviousButtons[2].gameObject.SetActive(isReversingTgl.isOn);
                nextPreviousButtons[3].gameObject.SetActive(isReversingTgl.isOn);
                soundEffectsTexts[1].text = "Reverse Gear sfx: " + reverseID + "/6";
            }
            // reversing beep warning sfx

            hasReverseBeepSound.interactable = isReversingTgl.isOn;
            if (hasReverseBeepSound.isOn)
            {
                nextPreviousButtons[4].gameObject.SetActive(isReversingTgl.isOn);
                nextPreviousButtons[5].gameObject.SetActive(isReversingTgl.isOn);
                soundEffectsTexts[2].text = "Reverse Beep sfx: " + reverseBeepID + "/4";
            }

            if (!isReversingTgl.isOn)
            {
                soundEffectsTexts[1].text = "Reverse Gear sfx";
                soundEffectsTexts[2].text = "Reverse Beep sfx";
            }
        }
        public void IsReverseGear(Toggle isGear)
        {
            for (int i = 0; i < allChildren.Length; i++)
            {
                if (allChildren[i].GetComponent<RealisticEngineSound>() != null)
                    allChildren[i].GetComponent<RealisticEngineSound>().enableReverseGear = isGear.isOn;
            }
            // gear
            nextPreviousButtons[2].gameObject.SetActive(isGear.isOn);
            nextPreviousButtons[3].gameObject.SetActive(isGear.isOn);
            if (isGear.isOn)
                // update text
                soundEffectsTexts[1].text = "Reverse Gear sfx: " + reverseID + "/6";
            else
                soundEffectsTexts[1].text = "Reverse Gear sfx";
        }
        public void IsReverseBeep(Toggle isBeep)
        {
            for (int i = 0; i < allChildren.Length; i++)
            {
                if (allChildren[i].GetComponent<RealisticEngineSound>() != null)
                    allChildren[i].GetComponent<RealisticEngineSound>().enableReverseBeep = isBeep.isOn;
            }
            // beep
            nextPreviousButtons[4].gameObject.SetActive(isBeep.isOn);
            nextPreviousButtons[5].gameObject.SetActive(isBeep.isOn);
            if (isBeep.isOn)
                // update text
                soundEffectsTexts[2].text = "Reverse Beep sfx: " + reverseBeepID + "/4";
            else
                soundEffectsTexts[2].text = "Reverse Beep sfx";
        }
        // is simulated rpm
        public void IsSimulated(Dropdown drpDown)
        {
            if (drpDown.value == 0)
            {
                simulated = true;
                accelerationSpeed.SetActive(true);
                gasPedalButton.SetActive(true);
            }
            if (drpDown.value == 1)
            {
                simulated = false;
                accelerationSpeed.SetActive(false);
                gasPedalButton.SetActive(false);
            }
        }
        // change car sound buttons
        public void ChangeCarSound(int a) // a = exterior, a+1 = interior prefabs id numbers in allChildren[]
        {
            // enable currently choosed sound pack and disable those that are not
            currentSoundpackID = a;
            if (isInterior)
                currentSoundpakcIDandCam = a + 1; // id for interior
            else
                currentSoundpakcIDandCam = a;
            for (int i = 0; i < allChildren.Length; i++)
            {
                if (allChildren[i].GetComponent<RealisticEngineSound>() != null)
                {
                    if (i != a && i != a + 1)
                        allChildren[i].GetComponent<RealisticEngineSound>().enabled = false;
                }
                if (allChildren[a].GetComponent<RealisticEngineSound>() != null)
                    allChildren[a].GetComponent<RealisticEngineSound>().enabled = true;
                if (allChildren[a + 1].GetComponent<RealisticEngineSound>() != null)
                    allChildren[a + 1].GetComponent<RealisticEngineSound>().enabled = true;
            }
            // change buttons color
            carSoundButtons[a / 2].color = Color.green; // set the choosed button's color to green
            for (int i = 0; i < carSoundButtons.Length; i++)
            {
                if (i * 2 != a) // set all other buttons color to white
                    carSoundButtons[i].color = Color.white;
            }
        }
        public void ExteriorInterior(bool a) // true = interior false = exterior
        {
            if (a)
            {
                isInterior = true;
                // turn off all exterior prefabs
                for (int i = 0; i < allChildren.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        allChildren[i].gameObject.SetActive(false); // exterior
                        allChildren[i + 1].gameObject.SetActive(true); // interior
                    }
                }
                currentSoundpakcIDandCam = currentSoundpackID + 1; // id for interior soundpacks
            }
            else
            {
                isInterior = false;
                // turn off all interior prefabs
                for (int i = 0; i < allChildren.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        allChildren[i].gameObject.SetActive(true); // exterior
                        allChildren[i + 1].gameObject.SetActive(false); // interior
                    }
                }
                currentSoundpakcIDandCam = currentSoundpackID; // id for exterior soundpacks
            }
        }
        // gas pedal checkbox
        public void UpdateGasPedal(Toggle togl)
        {
            if (allChildren[currentSoundpakcIDandCam].GetComponent<RealisticEngineSound>() != null)
                allChildren[currentSoundpakcIDandCam].GetComponent<RealisticEngineSound>().gasPedalPressing = togl.isOn;
        }
        public void WindSound() // enable/disable rolling sound
        {
            for (int i = 0; i < allChildren.Length; i++)
            {
                if (hasWindSound.isOn)
                    allChildren[i].windNoiseEnabled = DynamicSoundController.WindNoiseEnum.On;
                else
                    allChildren[i].windNoiseEnabled = DynamicSoundController.WindNoiseEnum.Off;
            }
            nextPreviousButtons[0].gameObject.SetActive(hasWindSound.isOn);
            nextPreviousButtons[1].gameObject.SetActive(hasWindSound.isOn);
            if (hasWindSound.isOn)
                // update text
                soundEffectsTexts[0].text = "Wind Noise: " + windID + "/5";
            else
                soundEffectsTexts[0].text = "Wind Noise";
        }
        public void ChangeWindClip(bool nextOrPrevious) // true = next, false = previous
        {
            if (!nextOrPrevious) // previous sound clip
            {
                if (windID > 1)
                {
                    windID -= 1;
                    for (int i = 0; i < allChildren.Length; i++)
                    {
                        allChildren[i].windClipID = windID;
                        if (i % 2 == 0) // exterior
                            allChildren[i].windNoiseClip = windClips[(windID - 1)];
                        if (i % 2 == 1) // interior
                            allChildren[i].windNoiseClip = windClips[(windID - 1 + 5)]; // + 5 are interior sound clips
                    }
                    nextPreviousButtons[0].interactable = true; // enable button
                    nextPreviousButtons[1].interactable = true; // enable button
                }
                if (windID == 1)
                {
                    nextPreviousButtons[0].interactable = false; // disable button
                }
            }
            else // next sound clip
            {
                if (windID < 6)
                {
                    windID += 1;
                    for (int i = 0; i < allChildren.Length; i++)
                    {
                        allChildren[i].windClipID = windID;
                        if (i % 2 == 0) // exterior
                            allChildren[i].windNoiseClip = windClips[(windID - 1)];
                        if (i % 2 == 1) // interior
                            allChildren[i].windNoiseClip = windClips[(windID - 1 + 6)]; // + 5 are interior sound clips
                    }
                    nextPreviousButtons[0].interactable = true; // enable button
                    nextPreviousButtons[1].interactable = true; // enable button
                }
                if (windID == 6)
                {
                    nextPreviousButtons[1].interactable = false; // disable button
                }
            }
            // update text
            soundEffectsTexts[0].text = "Wind Noise: " + windID + "/6";
        }
        public void ChangeReversingClip(bool nextOrPrevious) // true = next, false = previous
        {
            if (!nextOrPrevious) // previous sound clip
            {
                if (reverseID > 1)
                {
                    reverseID -= 1;
                    for (int i = 0; i < allChildren.Length; i++)
                    {
                        allChildren[i].reverseClipID = reverseID;
                        if (i % 2 == 0) // exterior
                            allChildren[i].reversingClip = reverseClips[(reverseID - 1)];
                        if (i % 2 == 1) // interior
                            allChildren[i].reversingClip = reverseClips[(reverseID - 1 + 6)]; // + 6 are interior sound clips
                    }
                    nextPreviousButtons[2].interactable = true; // enable button
                    nextPreviousButtons[3].interactable = true; // enable button
                }
                if (reverseID == 1)
                {
                    nextPreviousButtons[2].interactable = false; // disable button
                }
            }
            else // next sound clip
            {
                if (reverseID < 6)
                {
                    reverseID += 1;
                    for (int i = 0; i < allChildren.Length; i++)
                    {
                        allChildren[i].reverseClipID = reverseID;
                        if (i % 2 == 0) // exterior
                            allChildren[i].reversingClip = reverseClips[(reverseID - 1)];
                        if (i % 2 == 1) // interior
                            allChildren[i].reversingClip = reverseClips[(reverseID - 1 + 6)]; // + 6 are interior sound clips
                    }
                    nextPreviousButtons[2].interactable = true; // enable button
                    nextPreviousButtons[3].interactable = true; // enable button
                }
                if (reverseID == 6)
                {
                    nextPreviousButtons[3].interactable = false; // disable button
                }
            }
            // update text
            soundEffectsTexts[1].text = "Reverse Gear sfx: " + reverseID + "/6";
        }
        public void ChangeReversingBeepClip(bool nextOrPrevious) // true = next, false = previous
        {
            if (!nextOrPrevious) // previous sound clip
            {
                if (reverseBeepID > 1)
                {
                    reverseBeepID -= 1;
                    for (int i = 0; i < allChildren.Length; i++)
                    {
                        allChildren[i].reverseBeepClipID = reverseBeepID;
                        if (i % 2 == 0) // exterior
                            allChildren[i].reversingBeepClip = reverseBeepClips[(reverseBeepID - 1)];
                        if (i % 2 == 1) // interior
                            allChildren[i].reversingBeepClip = reverseBeepClips[(reverseBeepID - 1 + 4)]; // + 4 are interior sound clips
                    }
                    nextPreviousButtons[4].interactable = true; // enable button
                    nextPreviousButtons[5].interactable = true; // enable button
                }
                if (reverseBeepID == 1)
                {
                    nextPreviousButtons[4].interactable = false; // disable button
                }
            }
            else // next sound clip
            {
                if (reverseBeepID < 4)
                {
                    reverseBeepID += 1;
                    for (int i = 0; i < allChildren.Length; i++)
                    {
                        allChildren[i].reverseBeepClipID = reverseBeepID;
                        if (i % 2 == 0) // exterior
                            allChildren[i].reversingBeepClip = reverseBeepClips[(reverseBeepID - 1)];
                        if (i % 2 == 1) // interior
                            allChildren[i].reversingBeepClip = reverseBeepClips[(reverseBeepID - 1 + 4)]; // + 4 are interior sound clips
                    }
                    nextPreviousButtons[4].interactable = true; // enable button
                    nextPreviousButtons[5].interactable = true; // enable button
                }
                if (reverseBeepID == 4)
                {
                    nextPreviousButtons[5].interactable = false; // disable button
                }
            }
            // update text
            soundEffectsTexts[2].text = "Reverse Beep sfx: " + reverseBeepID + "/4";
        }
    }
}
