using System;
using System.Collections.Generic;
using UnityEngine;

using NoxCore.Fittings.Weapons;
using NoxCore.Utilities;

namespace NoxCore.Controllers
{
    public sealed class FireGroupController : MonoBehaviour
    {
        [SerializeField] const int _numFireGroups = 1;
        public int NumFireGroups {  get { return _numFireGroups; } }

        [SerializeField] private List<FireGroup> _fireGroups = new List<FireGroup>();
        public List<FireGroup> FireGroups { get { return _fireGroups; } }

        public void setFireGroupNames(List<string> names)
        {
            for (int i = 0; i < names.Count; i++)
            {
                _fireGroups[i].Name = names[i].ToUpper();
            }
        }

        // finders
        public FireGroup findFireGroup(int fireGroupID)
        {
            if (fireGroupID < _fireGroups.Count)
            {
                return _fireGroups[fireGroupID];
            }

            return null;
        }

        public FireGroup findFireGroup(string fireGroupName)
        {
            foreach (FireGroup fireGroup in _fireGroups)
            {
                if (String.Equals(fireGroup.Name, fireGroupName, StringComparison.OrdinalIgnoreCase))
                {
                    return fireGroup;
                }
            }

            return null;
        }

        public FireGroup findFireGroup(Weapon weapon)
        {
            foreach(FireGroup fireGroup in _fireGroups)
            {
                if (fireGroup.containsWeapon(weapon))
                {
                    return fireGroup;
                }
            }

            return null;
        }

        // get/remove all weapons
        public List<Weapon> getAllWeapons(string fireGroupName)
        {
            FireGroup fireGroup = findFireGroup(fireGroupName);

            if (fireGroup != null)
            {
                return fireGroup.getAllWeapons();
            }

            return null;
        }

        public List<Weapon> getAllWeapons(int fireGroupID)
        {
            FireGroup fireGroup = findFireGroup(fireGroupID);

            if (fireGroup != null)
            {
                return fireGroup.getAllWeapons();
            }

            return null;
        }

        public void removeAllWeapons(string fireGroupName)
        {
            FireGroup fireGroup = findFireGroup(fireGroupName);

            if (fireGroup != null)
            {
                fireGroup.removeAllWeapons();
            }
        }

        public void removeAllWeapons(int fireGroupID)
        {
            FireGroup fireGroup = findFireGroup(fireGroupID);

            if (fireGroup != null)
            {
                fireGroup.removeAllWeapons();
            }
        }

        // add weapon
        public void addWeaponToFireGroup(Weapon weapon, FireGroup fireGroup)
        {
            if (weapon != null && fireGroup != null)
            {
                FireGroup prevFireGroup = findFireGroup(weapon);

                if (prevFireGroup != null)
                {
                    prevFireGroup.removeWeapon(weapon);
                }

                fireGroup.addWeapon(weapon);
            }
            else
            {
                if (weapon == null)
                {
                    D.warn("Controller: {0}", "No weapon selected to add to fire group " + fireGroup);
                }
                else
                {
                    D.warn("Controller: {0}", "No fire group selected to add weapon " + weapon.name + " to");
                }
            }
        }

        public void addWeaponToFireGroup(Weapon weapon, int fireGroupID)
        {
            if (weapon != null)
            {
                FireGroup fireGroup = findFireGroup(fireGroupID);

                if (fireGroup != null)
                {
                    addWeaponToFireGroup(weapon, fireGroup);
                }
            }
        }

        public void addWeaponToFireGroup(Weapon weapon, string fireGroupName)
        {
            if (weapon != null)
            {
                FireGroup fireGroup = findFireGroup(fireGroupName);

                if (fireGroup != null)
                {
                    addWeaponToFireGroup(weapon, fireGroup);
                }
            }
        }

        // remove weapon
        public void removeWeapon(Weapon weapon)
        {
            if (weapon != null)
            {
                FireGroup curFireGroup = findFireGroup(weapon);

                if (curFireGroup != null)
                {
                    curFireGroup.removeWeapon(weapon);
                }
            }
        }

        // add weapons
        public void addWeaponsToFireGroup(Weapon[] weapons, FireGroup fireGroup)
        {
            if (weapons.Length > 0 && fireGroup != null)
            {
                for (int i = 0; i < weapons.Length; i++)
                {
                    FireGroup prevFireGroup = findFireGroup(weapons[i]);

                    if (prevFireGroup != null)
                    {
                        prevFireGroup.removeWeapon(weapons[i]);
                    }

                    fireGroup.addWeapon(weapons[i]);
                }
            }
            else
            {
                if (weapons.Length == 0)
                {
                    D.warn("Controller: {0}", "No weapons selected to add to fire group " + fireGroup);
                }
                else
                {
                    D.warn("Controller: {0}", "No fire group selected to add weapons " + weapons.ToString() + " to");
                }
            }
        }

        public void addWeaponsToFireGroup(Weapon[] weapons, string fireGroupName)
        {
            if (weapons.Length > 0)
            {
                FireGroup fireGroup = findFireGroup(fireGroupName);

                if (fireGroup != null)
                {
                    addWeaponsToFireGroup(weapons, fireGroup);
                }
            }
        }

        public void addWeaponsToFireGroup(Weapon[] weapons, int fireGroupID)
        {
            if (weapons.Length > 0)
            {
                FireGroup fireGroup = findFireGroup(fireGroupID);

                if (fireGroup != null)
                {
                    addWeaponsToFireGroup(weapons, fireGroup);
                }
            }
        }

        public void addWeaponsToFireGroup(List<Weapon> weapons, FireGroup fireGroup)
        {
            addWeaponsToFireGroup(weapons.ToArray(), fireGroup);
        }

        public void addWeaponsToFireGroup(List<Weapon> weapons, string fireGroupName)
        {
            addWeaponsToFireGroup(weapons.ToArray(), fireGroupName);
        }

        public void addWeaponsToFireGroup(List<Weapon> weapons, int fireGroupID)
        {
            addWeaponsToFireGroup(weapons.ToArray(), fireGroupID);
        }

        //remove weapons
        public void removeWeapons(Weapon [] weapons)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                FireGroup curFireGroup = findFireGroup(weapons[i]);

                if (curFireGroup != null)
                {
                    curFireGroup.removeWeapon(weapons[i]);
                }
            }
        }

        public void removeWeapons(List<Weapon> weapons)
        {
            removeWeapons(weapons.ToArray());
        }

        // swap fire groups
        public void swapFireGroup(Weapon weapon, FireGroup curFireGroup, FireGroup destFireGroup)
        {
            if (weapon != null && curFireGroup != null && destFireGroup != null)
            {
                curFireGroup.removeWeapon(weapon);
                destFireGroup.addWeapon(weapon);
            }
        }

        public void swapFireGroup(Weapon weapon, string curFireGroupName, string destFireGroupName)
        {
            FireGroup curFireGroup = findFireGroup(curFireGroupName);
            FireGroup destFireGroup = findFireGroup(destFireGroupName);

            swapFireGroup(weapon, curFireGroup, destFireGroup);
        }

        public void swapFireGroup(Weapon weapon, int curFireGroupID, int destFireGroupID)
        {
            FireGroup curFireGroup = findFireGroup(curFireGroupID);
            FireGroup destFireGroup = findFireGroup(destFireGroupID);

            swapFireGroup(weapon, curFireGroup, destFireGroup);
        }
    }
}