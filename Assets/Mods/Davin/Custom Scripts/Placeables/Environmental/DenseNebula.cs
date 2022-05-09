using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using NoxCore.Buffs;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Fittings.Weapons;

using Davin.Buffs;

namespace Davin.Placeables
{
    public class DenseNebula : NoxObject2D
    {
        /*
        public Buff enteredNebula(Ship ship)
        {
            return new NebulaMaxSpeedBuff(ship.engines, BuffType.PASSIVE, BuffCalculationType.HYBRID, 1, 50, true, 0);
        }
        */

        // note: we have to manage the triggering in case the same object hits the trigger more than once per tick due to multiple contact points hitting the trigger at the same time adding/removing the buff multiple times
        Dictionary<int, RotatingTurretBuff> shipsInsideBuffs;

        void Start()
        {
            shipsInsideBuffs = new Dictionary<int, RotatingTurretBuff>();
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            //Debug.Log(collision.name + "has hit nebula");

            if (collision.tag == "Ship")
            {
                int id = collision.GetInstanceID();

                if (!shipsInsideBuffs.ContainsKey(id))
                {
                    Ship ship = collision.GetComponent<Structure>() as Ship;

                    //List<ILauncher> turrets = new List<ILauncher>();

                    //turrets = ship.getWeapons<ILauncher>().Cast<ILauncher>().ToList<ILauncher>();

                    //RotatingTurretBuff rotatingTurretBuff = new RotatingTurretBuff(turrets, BuffType.PASSIVE, 1, 50, true, 0);

                    //ship.BuffManager.addBuff(rotatingTurretBuff);

                    //shipsInsideBuffs.Add(id, rotatingTurretBuff);
                }
            }
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.tag == "Ship")
            {
                int id = collision.GetInstanceID();

                if (shipsInsideBuffs.ContainsKey(id))
                {
                    Ship ship = collision.GetComponent<Structure>() as Ship;

                    RotatingTurretBuff nebulaMaxSpeedBuff;

                    shipsInsideBuffs.TryGetValue(id, out nebulaMaxSpeedBuff);

                    ship.BuffManager.removeBuff(nebulaMaxSpeedBuff);

                    shipsInsideBuffs.Remove(id);
                }
            }
        }
    }
}
