using System.Collections.Generic;
using System;

using UnityEngine;

using NoxCore.Data;
using NoxCore.Placeables.Ships;

using Davin.Buffs;
using Davin.Fittings.Weapons;

namespace NoxCore.Placeables
{
    public class DenseIceCloud : NoxObject2D
    {
        [Header("Ice Cloud Buff")]

        public BuffData __buffData;
        [NonSerialized]
        protected BuffData _buffData;
        public BuffData BuffData { get { return _buffData; } set { _buffData = value; } }

        // TODO - check if this is still required
        // note: we have to manage the triggering in case the same object hits the trigger more than once per tick due to multiple contact points hitting the trigger at the same time adding/removing the buff multiple times
        Dictionary<int, NebulaMaxSpeedBuff> shipsInsideBuffs = new Dictionary<int, NebulaMaxSpeedBuff>();

        void Start()
        {
            BuffData = Instantiate(__buffData);
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            //D.log(collision.name + "has hit nebula");

            if (collision.tag == "Ship")
            {
                int id = collision.GetInstanceID();

                if (!shipsInsideBuffs.ContainsKey(id))
                {
                    Ship ship = collision.GetComponent<Structure>() as Ship;

                    // create and add speed buff to ship
                    NebulaMaxSpeedBuff nebulaMaxSpeedBuff = new NebulaMaxSpeedBuff(ship.engines, BuffData);

                    ship.BuffManager.addBuff(nebulaMaxSpeedBuff);

                    shipsInsideBuffs.Add(id, nebulaMaxSpeedBuff);

                    // re-arm the snowball throwers
                    List<ISnowballThrower> snowballThrowers = ship.getWeapons<ISnowballThrower>();

                    foreach(ISnowballThrower snowballThrower in snowballThrowers)
                    {
                        snowballThrower.Ammo = snowballThrower.WeaponData.MaxAmmo;
                    }
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

                    // remove speed buff from ship
                    NebulaMaxSpeedBuff nebulaMaxSpeedBuff;

                    shipsInsideBuffs.TryGetValue(id, out nebulaMaxSpeedBuff);

                    ship.BuffManager.removeBuff(nebulaMaxSpeedBuff);

                    shipsInsideBuffs.Remove(id);
                }
                else
                {
                    D.warn("This should never happen", collision.gameObject);
                }
            }
        }
    }
}
