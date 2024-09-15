using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Meters
{
    public class MeterDisplay
    {
        // in order from 0, 1, 2, ..., 9 (if decimal)
        public static Sprite[] numberSprites;

        public Text label;
        List<Meter> meters;
        float[] nextFlushTimes;

        public MeterDisplay(List<Meter> meters)
        {
            nextFlushTimes = new float[meters.Count];

            for (int i = 0; i < nextFlushTimes.Length; i++)
            {
                nextFlushTimes[i] = Time.realtimeSinceStartup + meters[i].measurePeriod;
            }
        }

        public void update()
        {
            for (int i = 0; i < meters.Count; i++)
            {
                if (Time.realtimeSinceStartup > nextFlushTimes[i])
                {
                    meters[i].refresh();
                }
            }
        }
    }
}