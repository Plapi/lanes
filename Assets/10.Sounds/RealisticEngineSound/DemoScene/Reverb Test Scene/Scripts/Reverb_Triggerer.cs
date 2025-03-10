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

namespace SkrilStudio
{
    public class Reverb_Triggerer : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            gameObject.GetComponent<AudioReverbZone>().enabled = true;
        }
        private void OnTriggerExit(Collider other)
        {
            gameObject.GetComponent<AudioReverbZone>().enabled = false;
        }
    }
}
