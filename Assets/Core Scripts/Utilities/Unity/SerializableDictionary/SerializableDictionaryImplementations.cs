using UnityEngine;
using UnityEngine.Events;
using System;

using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Fittings.Weapons;

namespace NoxCore.Utilities
{
    // ---------------
    //  String => Int
    // ---------------
    [Serializable]
    public class StringIntDictionary : SerializableDictionary<string, int> { }

    // ---------------
    //  GameObject => Float
    // ---------------
    [Serializable]
    public class GameObjectFloatDictionary : SerializableDictionary<GameObject, float> { }

    // ---------------
    //  string => LayerMask
    // ---------------
    [Serializable]
    public class StringLayerMaskDictionary : SerializableDictionary<string, LayerMask> { }

    // ---------------
    //  int => UnityEvent
    // ---------------
    [Serializable]
    public class IntUnityEventDictionary : SerializableDictionary<int, UnityEvent> { }

    // ---------------
    //  int => ScriptableObject
    // ---------------
    [Serializable]
    public class IntScriptableObjectDictionary : SerializableDictionary<int, ScriptableObject> { }

    // ---------------
    //  string => ScriptableObject
    // ---------------
    [Serializable]
    public class StringScriptableObjectDictionary : SerializableDictionary<string, ScriptableObject> { }

    // ---------------
    //  string => MonoBehaviour
    // ---------------
    [Serializable]
    public class StringMonoBehaviourDictionary : SerializableDictionary<string, MonoBehaviour> { }

    // ---------------
    //  string => GameObject
    // ---------------
    [Serializable]
    public class StringGameObjectDictionary : SerializableDictionary<string, GameObject> { }

    // ---------------
    //  StructureSocketInfo => Module
    // ---------------
    [Serializable]
    public class StructureSocketInfoModuleDictionary : SerializableDictionary<StructureSocketInfo, Module> { }

    // ---------------
    //  StructureSocketInfo => Weapon
    // ---------------
    [Serializable]
    public class StructureSocketInfoWeaponDictionary : SerializableDictionary<StructureSocketInfo, Weapon> { }
}