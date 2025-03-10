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
using UnityEngine.UI;

namespace SkrilStudio
{
    public class CarSimulator : MonoBehaviour
    { // this script was only made for the main demo scene for demonstration purposes only
        [Header("This script is only made for main DEMO scene")]
        public bool gasPedalPressing = false;
        public float maxRPM = 7000;
        public float idle = 900;
        public float rpm = 0;
        public float accelerationSpeed = 1000f;
        public float decelerationSpeed = 1200f;
        public Slider accelSlider;

        private void Start()
        {
            rpm = idle;
        }
        void Update()
        {
            if (gasPedalPressing)
            {
                if (rpm < maxRPM)
                    rpm = Mathf.Lerp(rpm, rpm + accelerationSpeed * accelSlider.value, Time.deltaTime);
                else
                    rpm = maxRPM;
            }
            else
            {
                if (rpm > idle)
                    rpm = Mathf.Lerp(rpm, rpm - decelerationSpeed * accelSlider.value, Time.deltaTime);
            }
        }
        public void OnPointerDownRaceButton()
        {
            gasPedalPressing = true;
        }
        public void OnPointerUpRaceButton()
        {
            gasPedalPressing = false;
        }
    }
}
