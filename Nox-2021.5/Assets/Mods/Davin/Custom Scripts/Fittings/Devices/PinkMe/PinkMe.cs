using UnityEngine;
using System.Collections;

using NoxCore.Data.Fittings;
using NoxCore.Fittings.Devices;

namespace Davin.Fittings.Devices
{
    public class PinkMe : Device, IPinkMe
    {
        public float pinkMeTime;
        protected Color origColour;
        private float pinkMeTimer;
        private bool pinkToggle;

        public KeyCode debugKey = KeyCode.D;

        public override void init(DeviceData deviceData = null)
        {
            base.init();

            origColour = structure.GetComponent<SpriteRenderer>().material.color;
        }

        public override void reset()
        {
            base.reset();

            structure.GetComponent<SpriteRenderer>().material.color = origColour;
            pinkMeTimer = 0;
        }
    
        public void pinkMeUp()
        {
            if (isActiveOn() == true && isFlippingActivation() == false)
            {
                pinkToggle = !pinkToggle;

                if (pinkToggle)
                {
                    structure.GetComponent<SpriteRenderer>().material.color = Color.magenta;
                }
                else
                {
                    structure.GetComponent<SpriteRenderer>().material.color = origColour;
                }
            }
        }

        public override void update()
        {
            base.update();

            if (Input.GetKeyDown(debugKey))
            {
                pinkMeUp();
            }
               

            /*
            if (isActiveOn() == true && isFlippingActivation() == false)
            {
                // do effect
                pinkMeTimer += Time.deltaTime;

                if (pinkMeTimer >= pinkMeTime)
                {
                    structure.GetComponent<SpriteRenderer>().material.color = Color.magenta;
                }
            }
            */
        }
    }
}