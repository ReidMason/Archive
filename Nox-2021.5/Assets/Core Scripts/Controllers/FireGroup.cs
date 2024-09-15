using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Weapons;
using NoxCore.Placeables;

namespace NoxCore.Controllers
{
    [Serializable]
    public class FireGroup
    {
        [SerializeField]
        private string _name;
        public string Name {  get { return _name; } set { _name = value; } }

        [SerializeField]
        private int _id;
        public int ID { get { return _id; } }

        [SerializeField]
        private (GameObject structure, GameObject system)? _target;
        public (GameObject structure, GameObject system)? Target { get { return _target; } set { _target = value; } }

        [SerializeField]
        private List<Weapon> weapons;

        GameObject targetPosition;

        public int getNumWeapons()
        {
            return weapons.Count;
        }

        public FireGroup(int id)
        {
            _id = id;
            weapons = new List<Weapon>();
        }

        public List<T> getAllWeaponsOfType<T>() where T : IWeapon
        {
            List<T> matchingWeapons = new List<T>();

            foreach (Weapon weapon in weapons)
            {
                T matchingWeapon = weapon.GetComponentInChildren<T>();

                if (matchingWeapon != null)
                {
                    matchingWeapons.Add(matchingWeapon);
                }
            }

            return matchingWeapons;
        }

        public List<Weapon> getAllWeapons()
        {
            return weapons;
        }

        public void removeAllWeapons()
        {
            foreach(Weapon weapon in weapons)
            {
                weapon.FireGroup = null;
            }

            weapons.Clear();
        }

        public void addWeapon(Weapon weapon)
        {
            if (weapon != null)
            {
                weapon.FireGroup = this;
                weapons.Add(weapon);
            }
        }

        public bool removeWeapon(Weapon weapon)
        {
            if (weapon != null)
            {
                weapon.FireGroup = null;
                return weapons.Remove(weapon);
            }

            return false;
        }

        public bool containsWeapon(Weapon weapon)
        {
            return weapons.Contains(weapon);
        }

        public bool containsWeapon<T>() where T : IWeapon
        {
            foreach(Weapon weapon in weapons)
            {
                if (weapon is T)
                {
                    return true;
                }
            }

            return false;
        }

        // targeting

        public void setTarget(Vector2 target)
        {
            if (targetPosition == null)
            {
                targetPosition = new GameObject();
            }

            targetPosition.transform.position = target;

            Target = (targetPosition, null);

            for (int weaponID = 0; weaponID < weapons.Count; weaponID++)
            {
                TargetableWeapon targetableWeapon = weapons[weaponID] as TargetableWeapon;

                if (targetableWeapon != null)
                {
                    targetableWeapon.acquireTarget(targetPosition, null);
                }
            }
        }

        public void setTarget(Structure structure)
        {
            Target = (structure.gameObject, null);

            for (int weaponID = 0; weaponID < weapons.Count; weaponID++)
            {
                TargetableWeapon targetableWeapon = weapons[weaponID] as TargetableWeapon;

                if (targetableWeapon != null)
                {
                    targetableWeapon.acquireTarget(structure.gameObject, null);
                }
            }
        }

        public void setTarget(Structure structure, Module system)
        {
            Target = (structure.gameObject, system.gameObject);

            for (int weaponID = 0; weaponID < weapons.Count; weaponID++)
            {
                TargetableWeapon targetableWeapon = weapons[weaponID] as TargetableWeapon;

                if (targetableWeapon != null)
                {
                    targetableWeapon.acquireTarget(structure.gameObject, system.gameObject);
                }
            }
        }

        public void setTarget((Structure structure, Module system) target)
        {
            Target = (target.structure.gameObject, target.system.gameObject);

            for (int weaponID = 0; weaponID < weapons.Count; weaponID++)
            {
                TargetableWeapon targetableWeapon = weapons[weaponID] as TargetableWeapon;

                if (targetableWeapon != null)
                {
                    targetableWeapon.acquireTarget(target.structure.gameObject, target.system.gameObject);
                }
            }
        }

        public void setTarget(Structure structure, IModule system)
        {
            GameObject moduleGO = system.getGameObject();

            Target = (structure.gameObject, moduleGO);

            for (int weaponID = 0; weaponID < weapons.Count; weaponID++)
            {
                TargetableWeapon targetableWeapon = weapons[weaponID] as TargetableWeapon;

                if (targetableWeapon != null)
                {
                    targetableWeapon.acquireTarget(structure.gameObject, moduleGO);
                }
            }
        }

        public void setTarget((Structure structure, IModule system) target)
        {
            GameObject moduleGO = target.system.getGameObject();

            Target = (target.structure.gameObject, moduleGO);

            for (int weaponID = 0; weaponID < weapons.Count; weaponID++)
            {
                TargetableWeapon targetableWeapon = weapons[weaponID] as TargetableWeapon;

                if (targetableWeapon != null)
                {
                    targetableWeapon.acquireTarget(target.structure.gameObject, moduleGO);
                }
            }
        }

        public void setTarget(GameObject structure, GameObject system)
        {
            Target = (structure, system);

            for (int weaponID = 0; weaponID < weapons.Count; weaponID++)
            {
                TargetableWeapon targetableWeapon = weapons[weaponID] as TargetableWeapon;

                if (targetableWeapon != null)
                {
                    targetableWeapon.acquireTarget(structure, system);
                }
            }
        }

        public void setTarget((GameObject structure, GameObject system) target)
        {
            Target = target;

            for (int weaponID = 0; weaponID < weapons.Count; weaponID++)
            {
                TargetableWeapon targetableWeapon = weapons[weaponID] as TargetableWeapon;

                if (targetableWeapon != null)
                {
                    targetableWeapon.acquireTarget(target.structure, target.system);
                }
            }
        }

        public void unacquireTarget()
        {
            Target = null;

            for (int weaponID = 0; weaponID < weapons.Count; weaponID++)
            {
                TargetableWeapon targetableWeapon = weapons[weaponID] as TargetableWeapon;

                if (targetableWeapon != null)
                {
                    targetableWeapon.unacquireTarget();
                }
            }
        }
    }
}