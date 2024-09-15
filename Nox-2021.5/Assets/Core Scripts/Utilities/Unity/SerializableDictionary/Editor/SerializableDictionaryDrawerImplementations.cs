using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;

using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Fittings.Weapons;

namespace NoxCore.Utilities
{
    // ---------------
    //  String => Int
    // ---------------
    [UnityEditor.CustomPropertyDrawer(typeof(StringIntDictionary))]
    public class StringIntDictionaryDrawer : SerializableDictionaryDrawer<string, int>
    {
        protected override SerializableKeyValueTemplate<string, int> GetTemplate()
        {
            return GetGenericTemplate<SerializableStringIntTemplate>();
        }
    }
    internal class SerializableStringIntTemplate : SerializableKeyValueTemplate<string, int> { }

    // ---------------
    //  GameObject => Float
    // ---------------
    [UnityEditor.CustomPropertyDrawer(typeof(GameObjectFloatDictionary))]
    public class GameObjectFloatDictionaryDrawer : SerializableDictionaryDrawer<GameObject, float>
    {
        protected override SerializableKeyValueTemplate<GameObject, float> GetTemplate()
        {
            return GetGenericTemplate<SerializableGameObjectFloatTemplate>();
        }
    }
    internal class SerializableGameObjectFloatTemplate : SerializableKeyValueTemplate<GameObject, float> { }

    // ---------------
    //  String => LayerMask
    // ---------------
    [UnityEditor.CustomPropertyDrawer(typeof(StringLayerMaskDictionary))]
    public class StringLayerMaskDictionaryDrawer : SerializableDictionaryDrawer<string, LayerMask>
    {
        protected override SerializableKeyValueTemplate<string, LayerMask> GetTemplate()
        {
            return GetGenericTemplate<SerializableStringLayerMaskTemplate>();
        }
    }
    internal class SerializableStringLayerMaskTemplate : SerializableKeyValueTemplate<string, LayerMask> { }

    // ---------------
    //  int => UnityEvent
    // ---------------
    [UnityEditor.CustomPropertyDrawer(typeof(IntUnityEventDictionary))]
    public class IntUnityEventDictionaryDrawer : SerializableDictionaryDrawer<int, UnityEvent>
    {
        protected override SerializableKeyValueTemplate<int, UnityEvent> GetTemplate()
        {
            return GetGenericTemplate<SerializableIntUnityEventTemplate>();
        }
    }
    internal class SerializableIntUnityEventTemplate : SerializableKeyValueTemplate<int, UnityEvent> { }

    // ---------------
    //  string => GameObject
    // ---------------
    [UnityEditor.CustomPropertyDrawer(typeof(StringGameObjectDictionary))]
    public class StringGameObjectDictionaryDrawer : SerializableDictionaryDrawer<string, GameObject>
    {
        protected override SerializableKeyValueTemplate<string, GameObject> GetTemplate()
        {
            return GetGenericTemplate<SerializableStringGameObjectTemplate>();
        }
    }
    internal class SerializableStringGameObjectTemplate : SerializableKeyValueTemplate<string, GameObject> { }

    // ---------------
    //  StructureSocketInfo => Module
    // ---------------
    [UnityEditor.CustomPropertyDrawer(typeof(StructureSocketInfoModuleDictionary))]
    public class SocketInfoModuleDictionaryDrawer : SerializableDictionaryDrawer<StructureSocketInfo, Module>
    {
        protected override SerializableKeyValueTemplate<StructureSocketInfo, Module> GetTemplate()
        {
            return GetGenericTemplate<SerializableSocketInfoModuleTemplate>();
        }
    }
    internal class SerializableSocketInfoModuleTemplate : SerializableKeyValueTemplate<StructureSocketInfo, Module> { }

    // ---------------
    //  StructureSocketInfo => Weapon
    // ---------------
    [UnityEditor.CustomPropertyDrawer(typeof(StructureSocketInfoWeaponDictionary))]
    public class SocketInfoWeaponDictionaryDrawer : SerializableDictionaryDrawer<StructureSocketInfo, Weapon>
    {
        protected override SerializableKeyValueTemplate<StructureSocketInfo, Weapon> GetTemplate()
        {
            return GetGenericTemplate<SerializableSocketInfoWeaponTemplate>();
        }
    }
    internal class SerializableSocketInfoWeaponTemplate : SerializableKeyValueTemplate<StructureSocketInfo, Weapon> { }
}