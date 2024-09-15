using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

using NoxCore.Builders;
using NoxCore.Controllers;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Fittings.Weapons;
using NoxCore.Placeables;
using NoxCore.Utilities;

[CustomEditor(typeof(StationBuilder), true)]
public class StationBuilderEditor : BuilderEditor
{
    StationBuilder stationBuilder;

    public MonoScript overrideStructureScript;
    public MonoScript overrideControllerScript;

    public SerializedProperty _prefab;
    public SerializedProperty _skin;
    public SerializedProperty _structureType;
    public SerializedProperty _controllerName;
    public SerializedProperty _structureName;
    public SerializedProperty _commandedBy;
    public SerializedProperty _generatesStats;
    public SerializedProperty _factionID;
    public SerializedProperty _factionName;
    public SerializedProperty _startSpot;
    public SerializedProperty _startRotation;
    public SerializedProperty _canBeDamaged;
    public SerializedProperty _canRespawn;
    public SerializedProperty _respawnsAtStartSpot;
    public SerializedProperty _devices;
    public SerializedProperty _modules;
    public SerializedProperty _weapons;

    protected void updateControllerScript(Type contType)
    {
        GameObject temp = new GameObject();
        MonoBehaviour contComp = temp.AddComponent(contType) as MonoBehaviour;

        if (contComp != null)
        {
            overrideControllerScript = MonoScript.FromMonoBehaviour(contComp);
        }

        DestroyImmediate(temp);
    }

    public override void OnEnable()
    {
        base.OnEnable();

        stationBuilder = target as StationBuilder;

        _prefab = serializedObject.FindProperty("prefab");
        _controllerName = serializedObject.FindProperty("overrideControllerName");
        _skin = serializedObject.FindProperty("overrideSkin");
        _structureName = serializedObject.FindProperty("overrideStructureName");
        _commandedBy = serializedObject.FindProperty("overrideStructureCommandedBy");
        _generatesStats = serializedObject.FindProperty("overrideGeneratesStats");
        _factionID = serializedObject.FindProperty("overrideFactionID");
        _factionName = serializedObject.FindProperty("overrideFactionName");
        _startSpot = serializedObject.FindProperty("overrideStartSpot");
        _startRotation = serializedObject.FindProperty("overrideStartRotation");
        _canBeDamaged = serializedObject.FindProperty("overrideCanBeDamaged");
        _canRespawn = serializedObject.FindProperty("overrideCanRespawn");
        _respawnsAtStartSpot = serializedObject.FindProperty("overrideRespawnsAtStartSpot");
        _devices = serializedObject.FindProperty("overrideDevices");
        _modules = serializedObject.FindProperty("overrideModules");
        _weapons = serializedObject.FindProperty("overrideWeapons");

        GameObject stationPrefabGO = _prefab.objectReferenceValue as GameObject;

        if (stationPrefabGO != null)
        {
            Station prefabStructure = stationPrefabGO.GetComponent<Station>();
            if (prefabStructure != null) overrideStructureScript = MonoScript.FromMonoBehaviour(prefabStructure);

            //if (_controllerType == null)
            //{
            //StructureController prefabController = shipPrefabGO.GetComponent<StructureController>();
            //if (prefabController != null) overrideControllerScript = MonoScript.FromMonoBehaviour(prefabController);
            //}
            //else
            //{
            if (_controllerName.stringValue.Length > 0)
            {
                updateControllerScript(System.Type.GetType(_controllerName.stringValue + ",Assembly-CSharp"));
            }
            //}
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (showSettings)
        {
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_prefab, new GUIContent("Prefab"), GUILayout.MaxWidth(400));
            EditorGUILayout.PropertyField(_skin, new GUIContent("Skin"), GUILayout.MaxWidth(400));
            
            // sadly this doesn't work to set the Structure component directly if _structure is a Structure field (assigning scripts cannot be done like this)
            // EditorGUILayout.PropertyField(_structure, new GUIContent("Structure"), true, GUILayout.MaxWidth(450));

            overrideStructureScript = EditorGUILayout.ObjectField("Structure", overrideStructureScript, typeof(MonoScript), false, GUILayout.MaxWidth(400)) as MonoScript;

            if (overrideStructureScript != null)
            {
                Type structureType = overrideStructureScript.GetClass();

                if (!(structureType.IsSubclassOf(typeof(Station))))
                {
                    overrideStructureScript = null;
                    stationBuilder.overrideStructureType = null;
                }
                else
                {
                    stationBuilder.overrideStructureType = structureType;
                }
            }

            Type contType = System.Type.GetType(serializedObject.FindProperty("overrideControllerName").stringValue + ",Assembly-CSharp");

            if (overrideControllerScript == null || overrideControllerScript.GetClass() != contType)
            {
                updateControllerScript(contType);
            }

            overrideControllerScript = EditorGUILayout.ObjectField("Controller", overrideControllerScript, typeof(MonoScript), false, GUILayout.MaxWidth(400)) as MonoScript;

            if (overrideControllerScript != null)
            {
                Type controllerType = overrideControllerScript.GetClass();

                if (!(controllerType.IsSubclassOf(typeof(StructureController))))
                {
                    overrideControllerScript = null;
                    stationBuilder.overrideControllerName = null;
                }
                else
                {
                    stationBuilder.overrideControllerName = controllerType.ToString();
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_structureName, new GUIContent("Structure Name"), GUILayout.MaxWidth(400));
            EditorGUILayout.PropertyField(_commandedBy, new GUIContent("Commanded By"), GUILayout.MaxWidth(400));
            EditorGUILayout.PropertyField(_factionID, new GUIContent("Faction ID"), GUILayout.MaxWidth(400));
            EditorGUILayout.PropertyField(_factionName, new GUIContent("Faction Name"), GUILayout.MaxWidth(400));

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_startSpot, new GUIContent("Spawn Location"), GUILayout.MaxWidth(400));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Slider(_startRotation, 0, 360, "Spawn Rotation", GUILayout.MaxWidth(250));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_canBeDamaged, new GUIContent("Can Be Damaged"), GUILayout.MaxWidth(200));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_canRespawn, new GUIContent("Can Respawn"), GUILayout.MaxWidth(200));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_respawnsAtStartSpot, new GUIContent("Respawns At Start Spot"), GUILayout.MaxWidth(200));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_generatesStats, new GUIContent("Generates Stats"), GUILayout.MaxWidth(200));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_devices, new GUIContent("Devices"), true, GUILayout.MaxWidth(400));

            for (int i = 0; i < _devices.arraySize; i++)
            {
                Module module = _devices.GetArrayElementAtIndex(i).objectReferenceValue as Module;

                if (module != null)
                {
                    _devices.GetArrayElementAtIndex(i).objectReferenceValue = null;
                }
            }

            EditorGUILayout.PropertyField(_modules, new GUIContent("Modules"), true, GUILayout.MaxWidth(600));

            StructureSocketInfoModuleDictionary serializedModulesObject = _modules.GetActualObject<StructureSocketInfoModuleDictionary>();

            List<StructureSocketInfo> socketInfos = new List<StructureSocketInfo>(serializedModulesObject.dictionary.Keys);

            foreach (StructureSocketInfo socketInfo in socketInfos)
            {
                Module module = serializedModulesObject.dictionary[socketInfo];

                // test if actually a module and is able to be fitted into socket
                if (module == null || module is Weapon)
                {
                    serializedModulesObject.dictionary[socketInfo] = null;
                }
                else
                {
                    module.init();

                    if (socketInfo.canInstall(module) == false)
                    {
                        serializedModulesObject.dictionary[socketInfo] = null;
                    }
                }
            }

            EditorGUILayout.PropertyField(_weapons, new GUIContent("Weapons"), true, GUILayout.MaxWidth(600));

            StructureSocketInfoWeaponDictionary serializedWeaponsObject = _weapons.GetActualObject<StructureSocketInfoWeaponDictionary>();

            socketInfos = new List<StructureSocketInfo>(serializedWeaponsObject.dictionary.Keys);

            foreach (StructureSocketInfo socketInfo in socketInfos)
            {
                Weapon weapon = serializedWeaponsObject.dictionary[socketInfo];

                // test if actually a weapon and is able to be fitted into socket
                if (weapon == null || !(weapon is Weapon))
                {
                    serializedWeaponsObject.dictionary[socketInfo] = null;
                }
                else
                {
                    weapon.init();

                    if (socketInfo.canInstall(weapon) == false)
                    {
                        serializedWeaponsObject.dictionary[socketInfo] = null;
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
