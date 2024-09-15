using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

using NoxCore.GamePatterns;

namespace Meters
{
    public class MeterManager : SingletonBase<MeterManager>
    {
        public BindingFlags flags;
        List<MeterDisplay> meterDisplays = new List<MeterDisplay>();

        public void init()
        {
            MonoBehaviour[] sceneActive = FindObjectsOfType<MonoBehaviour>();
            foreach (MonoBehaviour mono in sceneActive)
            {
                string monoName = mono.name;

                FieldInfo[] objectFields = mono.GetType().GetFields(flags);

                List<Meter> meters = new List<Meter>();

                for (int i = 0; i < objectFields.Length; i++)
                {
                    MeterAttribute attribute = Attribute.GetCustomAttribute(objectFields[i], typeof(MeterAttribute)) as MeterAttribute;
                    if (attribute != null)
                    {
                        Debug.Log("Name: " + objectFields[i].Name); // The name of the flagged variable.

                        Type myObjectType = mono.GetType();

                        Debug.Log(objectFields[i].GetValue(mono));

                        GameObject meterGO = new GameObject();

                        Meter meter = meterGO.AddComponent<Meter>();
                        meter.linkProperty(objectFields[i]);

//                        MeterDisplay.AddComponent<Meter>();
                    }
                }
            }
        }
    }
}