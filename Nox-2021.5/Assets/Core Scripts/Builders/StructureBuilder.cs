using UnityEngine;
using System.Collections.Generic;
using System;

using NoxCore.Controllers;
using NoxCore.Data;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Fittings.Weapons;
using NoxCore.Placeables;
using NoxCore.Utilities;

namespace NoxCore.Builders
{
    public class StructureBuilder : Builder, INewBuilder
    {
        static Transform hierarchy;

        protected string prefabPath;
        public GameObject prefab;
        protected Structure structure;

        public Sprite overrideSkin;
        public Type overrideStructureType;
        public string overrideControllerName;
        public string overrideStructureName;
        public CommanderData overrideStructureCommand;
        public bool overrideGeneratesStats;
        public int overrideFactionID;
        public string overrideFactionName;
        public Vector2 overrideStartSpot;
        public float overrideStartRotation;
        public bool overrideCanBeDamaged;
        public bool overrideCanRespawn;
        public bool overrideRespawnsAtStartSpot;

        public List<Device> overrideDevices = new List<Device>();
        public List<StructureSocketInfo> overrideSockets = new List<StructureSocketInfo>();
        public StructureSocketInfoModuleDictionary overrideModules = StructureSocketInfoModuleDictionary.New<StructureSocketInfoModuleDictionary>();
        public StructureSocketInfoWeaponDictionary overrideWeapons = StructureSocketInfoWeaponDictionary.New<StructureSocketInfoWeaponDictionary>();

        void Awake()
        {
            setBuildType(BuildType.STRUCTURE);
        }

        protected Transform getPlacablesTrans()
        {
            if (hierarchy == null)
            {
                hierarchy = GameObject.Find("Placeables").transform;
            }

            return hierarchy;
        }

        public virtual void initialise(Structure structure)
        {
            structure.init();

            // add to scene hierarchy            
            structure.gameObject.transform.parent = getPlacablesTrans();

            // set overrides for structure info
            structure.Command = overrideStructureCommand;
//            structure.FactionID = overrideFactionID;
//            structure.FactionName = overrideFactionName;

            structure.CanBeDamaged = overrideCanBeDamaged;
            //structure.StructureData.CanRespawn = overrideCanRespawn;
            structure.StructureData.RespawnsAtStartSpot = overrideRespawnsAtStartSpot;
        }

        public void addDevice(string resourcePath)
        {
            Device devicePrefab = Resources.Load<Device>("Devices/" + resourcePath);

            if (devicePrefab == null)
            {
                D.warn("Device: {0}", "Cannot find a device prefab to build from in any Resources/Devices folder with path: " + resourcePath);
                return;
            }
            else
            {
                overrideDevices.Add(devicePrefab);
            }
        }
        /*
        public void addModule(string socketLabel, string resourcePath)
        {
            StructureSocketInfo socketInfo = structure.findDefaultSocketByName(socketLabel);

            if (socketInfo != null)
            {
                GameObject modulePrefab = Resources.Load<GameObject>("Modules/" + resourcePath);

                if (modulePrefab == null)
                {
                    D.warn("Module: {0}", "Cannot find a module prefab to build from in any Resources/Modules folder with path: " + resourcePath);

                    overrideModules.dictionary.Add(socketInfo, null);
                }
                else
                {
                    Module module = modulePrefab.GetComponent<Module>();

                    if (module != null)
                    {
                        module.init();

                        if (socketInfo.canInstall(module) == true)
                        {
                            overrideModules.dictionary.Add(socketInfo, module);

                            module.transform.parent = socketInfo.parent;
                        }
                        else
                        {
                            D.warn("Fitting: {0}", "Could not install module " + module.getType() + " into socket " + socketInfo.label);
                            overrideModules.dictionary.Add(socketInfo, null);
                        }
                    }
                    else
                    {
                        D.warn("Module: {0}", "No module to install into socket " + socketInfo.label);
                        overrideModules.dictionary.Add(socketInfo, null);
                    }
                }
            }
            else
            {
                D.warn("Socket: {0}", "No structure socket named " + socketLabel + " to build from to install module into. Check you have created the required socket prefab and built a default socket from it in your custom structure file before attempting to add a module to it.");
            }
        }
        */
        /*
        public void addWeapon(string socketLabel, string resourcePath)
        {
            StructureSocketInfo socketInfo = structure.findDefaultSocketByName(socketLabel);

            if (socketInfo != null)
            {
                GameObject weaponPrefab = Resources.Load<GameObject>("Weapons/" + resourcePath);

                if (weaponPrefab == null)
                {
                    D.warn("Weapon: {0}", "Cannot find a weapon prefab in any Resources/Weapons folder with path: " + resourcePath);

                    overrideWeapons.dictionary.Add(socketInfo, null);
                }
                else
                {
                    Weapon weapon = weaponPrefab.GetComponent<Weapon>();

                    if (weapon != null)
                    {
                        weapon.init();

                        if (socketInfo.canInstall(weapon) == true)
                        {
                            overrideWeapons.dictionary.Add(socketInfo, weapon);

                            weapon.transform.parent = socketInfo.parent;
                        }
                        else
                        {
                            D.warn("Fitting: {0}", "Could not install weapon " + weapon.getType() + " into socket " + socketInfo.label);
                            overrideWeapons.dictionary.Add(socketInfo, null);
                        }
                    }
                    else
                    {
                        D.warn("Weapon: {0}", "No weapon to install into socket " + socketInfo.label);
                        overrideWeapons.dictionary.Add(socketInfo, null);
                    }
                }
            }
            else
            {
                D.warn("Socket: {0}", "No structure socket named " + socketLabel + " to build from to install weapon into. Check you have created the required socket prefab and built a default socket from it in your custom structure file before attempting to add a weapon to it.");
            }
        }
        */

        public override void preBuild()
        {
            if (prefab != null)
            {
                // destroy any controller attached to the prefab (these should never be on the prefabs and could cause issues)
                Destroy(prefab.GetComponent<StructureController>());
            }
        }

        public virtual Structure setupStructure(GameObject go)
        {
            Structure structure = go.GetComponent<Structure>();

            if (overrideStructureType != null && overrideStructureType != structure.GetType())
            {
                // remove existing structure from new GameObject (note: not the prefab)
                if (structure != null) Destroy(structure);

                // add overridden structure
                structure = go.AddComponent(overrideStructureType) as Structure;
            }

            if (structure == null)
            {
                D.log("Structure", "No structure component added to the prefab at: " + prefabPath);
                Destroy(go);
                return null;
            }
            else
            {
                initialise(structure);
            }

            return structure;
        }

        public virtual StructureController setupController(GameObject go, Structure structure)
        {
            StructureController controller = go.GetComponent<StructureController>();

            if (overrideControllerName != null)
            {
                if (controller == null)
                {
                    controller = go.AddComponent(System.Type.GetType(overrideControllerName)) as StructureController;
                }
                else
                {
                    if (System.Type.GetType(overrideControllerName) != controller.GetType())
                    {
                        // remove existing controller from ship (note: not the prefab)
                        Destroy(controller);

                        // add overridden controller
                        controller = go.AddComponent(System.Type.GetType(overrideControllerName)) as StructureController;
                    }
                }
            }

            if (controller != null)
            {
                structure.attachController(controller);

                controller.startSpot = overrideStartSpot;
                controller.startRotation = overrideStartRotation;

                controller.GeneratesStats = overrideGeneratesStats;
            }

            return controller;
        }

        public virtual void setupFittings(Structure structure)
        {
            // Devices
            foreach (Device device in overrideDevices)
            {
                if (device != null) structure.addDevice(device.gameObject, structure.transform.Find("Systems"));
            }

            // Modules
            foreach (KeyValuePair<StructureSocketInfo, Module> socketInfoModule in overrideModules.dictionary)
            {
                if (socketInfoModule.Value != null) structure.addModule(socketInfoModule.Key.label, socketInfoModule.Value.gameObject);
            }

            foreach (KeyValuePair<StructureSocketInfo, Weapon> socketInfoWeapon in overrideWeapons.dictionary)
            {
                if (socketInfoWeapon.Value != null) structure.addWeapon(socketInfoWeapon.Key.label, socketInfoWeapon.Value.gameObject);
            }
        }
    }
}