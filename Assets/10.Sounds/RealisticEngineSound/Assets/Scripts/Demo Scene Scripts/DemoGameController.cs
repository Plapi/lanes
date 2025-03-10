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
using UnityEngine.UI;

namespace SkrilStudio
{
    public class DemoGameController : MonoBehaviour // this script is only made for the car controller demo scenes to change the sound packs on runtime, it is not required for the asset, will work without it
    {
        [Header("This script is only made for car controller DEMO scenes")]
        SkrilStudio.RealisticEngineSound[] allChildren;
        public Image[] carSoundButtons;
        public Slider aggressivenessSlider;
        private int currentSoundpakcIDandCam = 2; // id to choose a sound pack for scene start
        private WaitForSeconds _wait;
        private void Start()
        {
            allChildren = GetComponentsInChildren<SkrilStudio.RealisticEngineSound>(true);
            _wait = new WaitForSeconds(0.2f); // setup wait for secconds
            StartCoroutine(WaitForStart());
        }
        // change car sound with buttons
        public void ChangeCarSound(int a)
        {           
            for (int i = 0; i < allChildren.Length; i++)
            {
                if (allChildren[i].GetComponent<SkrilStudio.RealisticEngineSound>() != null)
                {
                    if (i != a && i != a + 1)
                        allChildren[i].GetComponent<SkrilStudio.RealisticEngineSound>().enabled = false;
                }
            }
            if (allChildren[a].GetComponent<SkrilStudio.RealisticEngineSound>() != null)
                allChildren[a].GetComponent<SkrilStudio.RealisticEngineSound>().enabled = true;
            if (allChildren[a + 1].GetComponent<SkrilStudio.RealisticEngineSound>() != null)
                allChildren[a + 1].GetComponent<SkrilStudio.RealisticEngineSound>().enabled = true;

            carSoundButtons[a/2].color = Color.green; // set the choosed button's color to green
            for (int i = 0; i<carSoundButtons.Length; i++)
            {
                if (i*2!= a) // set all other buttons color to white
                    carSoundButtons[i].color = Color.white;
            }
            currentSoundpakcIDandCam = a;
        }
        private void Update()
        {
            allChildren[currentSoundpakcIDandCam].GetComponent<RealisticEngineSound>().aggressivnessMaster = aggressivenessSlider.value; // exterior prefab
            allChildren[currentSoundpakcIDandCam+1].GetComponent<RealisticEngineSound>().aggressivnessMaster = aggressivenessSlider.value; // interior prefab
        }
        public void ChangeOffload(Dropdown dropdown)
        {
            if (dropdown.value == 0)
                for (int i = 0; i < allChildren.Length; i++)
                    allChildren[i].GetComponent<RealisticEngineSound>().offLoadType = RealisticEngineSound.OffLoadType.Prerecorded;
            if (dropdown.value == 1)
                for (int i = 0; i < allChildren.Length; i++)
                    allChildren[i].GetComponent<RealisticEngineSound>().offLoadType = RealisticEngineSound.OffLoadType.Simulated;
        }
        IEnumerator WaitForStart() // give some time for the car controller to startup
        {
            while (true)
            {
                yield return _wait;
                ChangeCarSound(currentSoundpakcIDandCam);
                break;
            }
        }
    }   
}
