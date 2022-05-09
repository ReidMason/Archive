using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

// https://blog.gemserk.com/2017/03/14/using-unity-text-to-show-numbers-without-garbage-generation/
// http://www.ennoble-studios.com/tuts/fps-coutner/

namespace Meters
{
    public class Meter : MonoBehaviour
    {
        public FieldInfo fieldInfo;
        public Image[] numbers;
        public float measurePeriod;
        public bool fillZero;
        // colour, animated etc.

        void Awake()
        {
            // cache number images (note: will this get them in the right order? May need to sort based on ID or similar)
            numbers = GetComponentsInChildren<Image>();
        }

        public void linkProperty(FieldInfo fieldInfo)
        {
            this.fieldInfo = fieldInfo;
        }

        public void refresh()
        {
            // demo - get from actual property/field
            setNumber((int)(Random.value * 10000));
        }

        protected void setNumber(int number)
        {
            int digit;
            int modulus = 10;
            bool digitActive;

            for (int i = 0; i < numbers.Length; i++)
            {
                digit = (number % modulus) / (modulus / 10);

                if (i == 0)
                {
                    digitActive = fillZero || digit > 0;
                }
                else
                {
                    digitActive = fillZero || digit != 0;
                }

                numbers[i].gameObject.SetActive(digitActive);

                modulus *= 10;
            }

            /*
            int tenThousands = (number % 100000) / 10000;
            int thousands = (number % 10000) / 1000;
            int hundreds = (number % 1000) / 100;
            int tens = (number % 100) / 10;
            int ones = (number % 10);

            var tenThousandsActive = fillZero || tenThousands != 0;
            var thousandsActive = fillZero || thousands != 0;
            var hundredsActive = fillZero || hundreds != 0;
            var tensActive = fillZero || tens != 0;
            var onesActive = fillZero || number > 0;
            */

            /*
            numbers[0].gameObject.SetActive(tenThousandsActive);
            numbers[1].gameObject.SetActive(thousandsActive);
            numbers[2].gameObject.SetActive(hundredsActive);
            numbers[3].gameObject.SetActive(tensActive);
            numbers[4].gameObject.SetActive(onesActive);
            */
        }
    }
}